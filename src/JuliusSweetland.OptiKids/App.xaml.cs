using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Observables.PointSources;
using JuliusSweetland.OptiKids.Observables.TriggerSources;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using JuliusSweetland.OptiKids.UI.ViewModels;
using JuliusSweetland.OptiKids.UI.Views;
using JuliusSweetland.OptiKids.UI.Windows;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NBug.Core.UI;
using Newtonsoft.Json;
using Octokit;
using Octokit.Reactive;
using Application = System.Windows.Application;
using FileMode = System.IO.FileMode;

namespace JuliusSweetland.OptiKids
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constants

        private const string GazeTrackerUdpRegex = @"^STREAM_DATA\s(?<instanceTime>\d+)\s(?<x>-?\d+(\.[0-9]+)?)\s(?<y>-?\d+(\.[0-9]+)?)";

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Action applyTheme;
        private readonly MainWindow mainWindow = new MainWindow();

        #endregion

        #region Ctor

        public App()
        {
            //Setup unhandled exception handling and NBug
            AttachUnhandledExceptionHandlers(mainWindow);

            //Log startup diagnostic info
            Log.Info("STARTING UP.");

            //Attach shutdown handler
            Current.Exit += (o, args) =>
            {
                Log.Info("PERSISTING USER SETTINGS AND SHUTTING DOWN.");
                Settings.Default.Save();
            };

            HandleCorruptSettings();

            //Upgrade settings (if required) - this ensures that user settings migrate between version changes
            if (Settings.Default.SettingsUpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgradeRequired = false;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            //Adjust log4net logging level if in debug mode
            ((Hierarchy)LogManager.GetRepository()).Root.Level = Settings.Default.Debug ? Level.Debug : Level.Info;
            ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);

            //Logic to initially apply the theme and change the theme on setting changes
            applyTheme = () =>
            {
                var themeDictionary = new ThemeResourceDictionary
                {
                    Source = new Uri(Settings.Default.Theme, UriKind.Relative)
                };
                
                var previousThemes = Resources.MergedDictionaries
                    .OfType<ThemeResourceDictionary>()
                    .ToList();
                    
                //N.B. Add replacement before removing the previous as having no applicable resource
                //dictionary can result in the first element not being rendered (usually the first key).
                Resources.MergedDictionaries.Add(themeDictionary);
                previousThemes.ForEach(rd => Resources.MergedDictionaries.Remove(rd));
            };
            
            Settings.Default.OnPropertyChanges(settings => settings.Theme).Subscribe(_ => applyTheme());
        }

        #endregion

        #region On Startup

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Log.Info("Boot strapping the services and UI.");

                //Apply theme
                applyTheme();

                //Create services
                var errorNotifyingServices = new List<INotifyErrors>();
                IAudioService audioService = new AudioService();
                IKeyStateService keyStateService = new KeyStateService();
                IInputService inputService = CreateInputService();
                errorNotifyingServices.Add(audioService);
                errorNotifyingServices.Add(inputService);

                //Load pronunciation and quiz from files
                var pronunciation = LoadPronunciation();
                
                //Compose UI
                var mainViewModel = new MainViewModel(audioService, inputService, 
                    keyStateService, errorNotifyingServices, pronunciation);
                var mainView = new MainView {DataContext = mainViewModel};
                mainWindow.Content = mainView;

                //Setup actions to take once main view is loaded (i.e. the view is ready, so hook up the services which kicks everything off)
                Action postMainViewLoaded = () =>
                {
                    mainViewModel.AttachServiceEventHandlers();
                };
                if(mainView.IsLoaded)
                {
                    postMainViewLoaded();
                }
                else
                {
                    RoutedEventHandler loadedHandler = null;
                    loadedHandler = (s, a) =>
                    {
                        postMainViewLoaded();
                        mainView.Loaded -= loadedHandler; //Ensure this handler only triggers once
                    };
                    mainView.Loaded += loadedHandler;
                }

                //Show the main window
                mainWindow.Show();
                inputService.RequestResume(); //Start the input service
            }
            catch (Exception ex)
            {
                Log.Error("Error starting up application", ex);
                throw;
            }
        }

        #endregion

        #region Attach Unhandled Exception Handlers

        private static void AttachUnhandledExceptionHandlers(Window window)
        {
            Current.DispatcherUnhandledException += (sender, args) =>
            {
                Log.Error("A DispatcherUnhandledException has been encountered...", args.Exception);
                MessageBox.Show(window, "EXCEPTION!", args.Exception.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown();
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Error("An UnhandledException has been encountered...", args.ExceptionObject as Exception);
                MessageBox.Show(window, "EXCEPTION!", (args.ExceptionObject as Exception).Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown();
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error("An UnobservedTaskException has been encountered...", args.Exception);
                MessageBox.Show(window, "EXCEPTION!", args.Exception.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown();
            };
        }

        #endregion

        #region Handle Corrupt Settings

        private static void HandleCorruptSettings()
        {
            try
            {
                //Attempting to read a setting from a corrupt user config file throws an exception
                var upgradeRequired = Settings.Default.SettingsUpgradeRequired;
            }
            catch (ConfigurationErrorsException cee)
            {
                Log.Warn("User settings file is corrupt and needs to be corrected. Alerting user and shutting down.");
                string filename = ((ConfigurationErrorsException)cee.InnerException).Filename;

                if (MessageBox.Show(
                        OptiKids.Properties.Resources.CORRUPTED_SETTINGS_MESSAGE,
                        OptiKids.Properties.Resources.CORRUPTED_SETTINGS_TITLE,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    File.Delete(filename);
                    try
                    {
                        System.Windows.Forms.Application.Restart();
                    }
                    catch {} //Swallow any exceptions (e.g. DispatcherExceptions) - we're shutting down so it doesn't matter.
                }
                Current.Shutdown(); //Avoid the inevitable crash by shutting down gracefully
            }
        }

        #endregion

        #region Create Service Methods
        
        private static IInputService CreateInputService()
        {
            Log.Info("Creating InputService.");

            //Instantiate point source
            IPointSource pointSource;
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.GazeTracker:
                    pointSource = new GazeTrackerSource(
                        Settings.Default.PointTtl,
                        Settings.Default.GazeTrackerUdpPort,
                        new Regex(GazeTrackerUdpRegex));
                    break;

                case PointsSources.MousePosition:
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);
                    break;

                case PointsSources.TheEyeTribe:
                    var theEyeTribePointService = new TheEyeTribePointService();
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        theEyeTribePointService);
                    break;

                case PointsSources.TobiiEyeX:
                case PointsSources.TobiiRex:
                case PointsSources.TobiiPcEyeGo:
                    var tobiiEyeXPointService = new TobiiEyeXPointService();
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        tobiiEyeXPointService);
                    break;

                case PointsSources.VisualInteractionMyGaze:
                    var myGazePointService = new MyGazePointService();
                    pointSource = new PointServiceSource(
                        Settings.Default.PointTtl,
                        myGazePointService);
                    break;

                default:
                    throw new ArgumentException("'PointsSource' settings is missing or not recognised! Please correct and restart OptiKids.");
            }

            //Instantiate key trigger source
            ITriggerSource keySelectionTriggerSource;
            switch (Settings.Default.KeySelectionTriggerSource)
            {
                case TriggerSources.Fixations:
                    keySelectionTriggerSource = new KeyFixationSource(
                       Settings.Default.KeySelectionTriggerFixationLockOnTime,
                       Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn,
                       Settings.Default.KeySelectionTriggerFixationCompleteTime,
                       Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                       pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    keySelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    keySelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.KeySelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
                    break;

                default:
                    throw new ArgumentException(
                        "'KeySelectionTriggerSource' setting is missing or not recognised! Please correct and restart OptiKids.");
            }
            
            var inputService = new InputService(pointSource, keySelectionTriggerSource);
            inputService.RequestSuspend(); //Pause it initially
            return inputService;
        }

        #endregion

        #region Load Pronunciation

        //https://msdn.microsoft.com/en-us/library/office/hh361601(v=office.14).aspx#PhoneTables
        private Dictionary<char, string> LoadPronunciation()
        {
            var pronunciationString = File.ReadAllText(Settings.Default.PronunciationFile);
            return JsonConvert.DeserializeObject<Dictionary<char, string>>(pronunciationString);
        }

        #endregion
    }
}
