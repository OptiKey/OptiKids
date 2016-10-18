using JuliusSweetland.OptiKids.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.UI.ViewModels.Management
{
    public class OtherViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public OtherViewModel()
        {
            Load();
        }

        #endregion

        #region Properties

        private bool checkForUpdates;
        public bool CheckForUpdates
        {
            get { return checkForUpdates; }
            set { SetProperty(ref checkForUpdates, value); }
        }

        private bool debug;
        public bool Debug
        {
            get { return debug; }
            set { SetProperty(ref debug, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                return Settings.Default.Debug != Debug;
            }
        }
        
        #endregion
        
        #region Methods

        private void Load()
        {
            CheckForUpdates = Settings.Default.CheckForUpdates;
            Debug = Settings.Default.Debug;
        }

        public void ApplyChanges()
        {
            Settings.Default.CheckForUpdates = CheckForUpdates;
            Settings.Default.Debug = Debug;
        }

        #endregion
    }
}
