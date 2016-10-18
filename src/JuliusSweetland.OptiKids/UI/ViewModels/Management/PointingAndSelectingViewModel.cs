using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.UI.ViewModels.Management
{
    public class PointingAndSelectingViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public PointingAndSelectingViewModel()
        {
            Load();

            //Set up property defaulting logic
            this.OnPropertyChanges(vm => vm.ProgressIndicatorBehaviour).Subscribe(pib =>
            {
                if (pib == Enums.ProgressIndicatorBehaviours.Grow &&
                    ProgressIndicatorResizeStartProportion > ProgressIndicatorResizeEndProportion)
                {
                    var endProportion = ProgressIndicatorResizeEndProportion;
                    ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeStartProportion;
                    ProgressIndicatorResizeStartProportion = endProportion;
                }
                else if (pib == Enums.ProgressIndicatorBehaviours.Shrink &&
                    ProgressIndicatorResizeStartProportion < ProgressIndicatorResizeEndProportion)
                {
                    var endProportion = ProgressIndicatorResizeEndProportion;
                    ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeStartProportion;
                    ProgressIndicatorResizeStartProportion = endProportion;
                }
            });
        }
        
        #endregion
        
        #region Properties

        public List<KeyValuePair<string, PointsSources>> PointsSources
        {
            get
            {
                return new List<KeyValuePair<string, PointsSources>>
                {
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.GazeTracker.ToDescription(), Enums.PointsSources.GazeTracker),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.MousePosition.ToDescription(), Enums.PointsSources.MousePosition),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TheEyeTribe.ToDescription(), Enums.PointsSources.TheEyeTribe),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiEyeX.ToDescription(), Enums.PointsSources.TobiiEyeX),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiRex.ToDescription(), Enums.PointsSources.TobiiRex),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.TobiiPcEyeGo.ToDescription(), Enums.PointsSources.TobiiPcEyeGo),
                    new KeyValuePair<string, PointsSources>(Enums.PointsSources.VisualInteractionMyGaze.ToDescription(), Enums.PointsSources.VisualInteractionMyGaze)
                };
            }
        }
        
        public List<KeyValuePair<string, TriggerSources>> TriggerSources
        {
            get
            {
                return new List<KeyValuePair<string, TriggerSources>>
                {
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.Fixations.ToDescription(), Enums.TriggerSources.Fixations),
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.KeyboardKeyDownsUps.ToDescription(), Enums.TriggerSources.KeyboardKeyDownsUps),
                    new KeyValuePair<string, TriggerSources>(Enums.TriggerSources.MouseButtonDownUps.ToDescription(), Enums.TriggerSources.MouseButtonDownUps)
                };
            }
        }

        public List<KeyValuePair<string, DataStreamProcessingLevels>> DataStreamProcessingLevels
        {
            get
            {
                return new List<KeyValuePair<string, DataStreamProcessingLevels>>
                {
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.None.ToDescription(), Enums.DataStreamProcessingLevels.None),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.Low.ToDescription(), Enums.DataStreamProcessingLevels.Low),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.Medium.ToDescription(), Enums.DataStreamProcessingLevels.Medium),
                    new KeyValuePair<string, DataStreamProcessingLevels>(Enums.DataStreamProcessingLevels.High.ToDescription(), Enums.DataStreamProcessingLevels.High)
                };
            }
        }
        
        public List<Keys> Keys
        {
            get { return Enum.GetValues(typeof(Enums.Keys)).Cast<Enums.Keys>().OrderBy(k => k.ToString()).ToList(); }
        }
        
        public List<MouseButtons> MouseButtons
        {
            get { return Enum.GetValues(typeof(Enums.MouseButtons)).Cast<Enums.MouseButtons>().OrderBy(mb => mb.ToString()).ToList(); }
        }
        
        public List<KeyValuePair<string, ProgressIndicatorBehaviours>> ProgressIndicatorBehaviours
        {
            get
            {
                return new List<KeyValuePair<string, ProgressIndicatorBehaviours>>
                {
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.FILL_PIE, Enums.ProgressIndicatorBehaviours.FillPie),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.GROW, Enums.ProgressIndicatorBehaviours.Grow),
                    new KeyValuePair<string, ProgressIndicatorBehaviours>(Resources.SHRINK_INDICATOR, Enums.ProgressIndicatorBehaviours.Shrink)
                };
            }
        }
        
        private PointsSources pointSource;
        public PointsSources PointsSource
        {
            get { return pointSource; }
            set { SetProperty(ref pointSource, value); }
        }

        private DataStreamProcessingLevels tobiiEyeXProcessingLevel;
        public DataStreamProcessingLevels TobiiEyeXProcessingLevel
        {
            get {  return tobiiEyeXProcessingLevel; }
            set { SetProperty(ref tobiiEyeXProcessingLevel, value); }
        }

        private double pointsMousePositionSampleIntervalInMs;
        public double PointsMousePositionSampleIntervalInMs
        {
            get { return pointsMousePositionSampleIntervalInMs; }
            set { SetProperty(ref pointsMousePositionSampleIntervalInMs, value); }
        }

        private double pointTtlInMs;
        public double PointTtlInMs
        {
            get { return pointTtlInMs; }
            set { SetProperty(ref pointTtlInMs, value); }
        }
        
        private TriggerSources keySelectionTriggerSource;
        public TriggerSources KeySelectionTriggerSource
        {
            get { return keySelectionTriggerSource; }
            set { SetProperty(ref keySelectionTriggerSource, value); }
        }

        private Keys keySelectionTriggerKeyboardKeyDownUpKey;
        public Keys KeySelectionTriggerKeyboardKeyDownUpKey
        {
            get { return keySelectionTriggerKeyboardKeyDownUpKey; }
            set { SetProperty(ref keySelectionTriggerKeyboardKeyDownUpKey, value); }
        }

        private MouseButtons keySelectionTriggerMouseDownUpButton;
        public MouseButtons KeySelectionTriggerMouseDownUpButton
        {
            get { return keySelectionTriggerMouseDownUpButton; }
            set { SetProperty(ref keySelectionTriggerMouseDownUpButton, value); }
        }
        
        private double keySelectionTriggerFixationLockOnTimeInMs;
        public double KeySelectionTriggerFixationLockOnTimeInMs
        {
            get { return keySelectionTriggerFixationLockOnTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationLockOnTimeInMs, value); }
        }

        private bool keySelectionTriggerFixationResumeRequiresLockOn;
        public bool KeySelectionTriggerFixationResumeRequiresLockOn
        {
            get { return keySelectionTriggerFixationResumeRequiresLockOn; }
            set { SetProperty(ref keySelectionTriggerFixationResumeRequiresLockOn, value); }
        }

        private double keySelectionTriggerFixationCompleteTimeInMs;
        public double KeySelectionTriggerFixationCompleteTimeInMs
        {
            get { return keySelectionTriggerFixationCompleteTimeInMs; }
            set { SetProperty(ref keySelectionTriggerFixationCompleteTimeInMs, value); }
        }
        
        private double keySelectionTriggerIncompleteFixationTtlInMs;
        public double KeySelectionTriggerIncompleteFixationTtlInMs
        {
            get { return keySelectionTriggerIncompleteFixationTtlInMs; }
            set { SetProperty(ref keySelectionTriggerIncompleteFixationTtlInMs, value); }
        }

        private ProgressIndicatorBehaviours progressIndicatorBehaviour;
        public ProgressIndicatorBehaviours ProgressIndicatorBehaviour
        {
            get { return progressIndicatorBehaviour; }
            set { SetProperty(ref progressIndicatorBehaviour, value); }
        }

        private int progressIndicatorResizeStartProportion;
        public int ProgressIndicatorResizeStartProportion
        {
            get { return progressIndicatorResizeStartProportion; }
            set { SetProperty(ref progressIndicatorResizeStartProportion, value); }
        }

        private int progressIndicatorResizeEndProportion;
        public int ProgressIndicatorResizeEndProportion
        {
            get { return progressIndicatorResizeEndProportion; }
            set { SetProperty(ref progressIndicatorResizeEndProportion, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                return Settings.Default.PointsSource != PointsSource
                    || (Settings.Default.TobiiEyeXProcessingLevel != TobiiEyeXProcessingLevel && PointsSource == Enums.PointsSources.TobiiEyeX)
                    || (Settings.Default.PointsMousePositionSampleInterval != TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs) && PointsSource == Enums.PointsSources.MousePosition)
                    || Settings.Default.PointTtl != TimeSpan.FromMilliseconds(PointTtlInMs)
                    || Settings.Default.KeySelectionTriggerSource != KeySelectionTriggerSource
                    || (Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey != KeySelectionTriggerKeyboardKeyDownUpKey && KeySelectionTriggerSource == Enums.TriggerSources.KeyboardKeyDownsUps)
                    || (Settings.Default.KeySelectionTriggerMouseDownUpButton != KeySelectionTriggerMouseDownUpButton && KeySelectionTriggerSource == Enums.TriggerSources.MouseButtonDownUps)
                    || (Settings.Default.KeySelectionTriggerFixationLockOnTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn != KeySelectionTriggerFixationResumeRequiresLockOn && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerFixationCompleteTime != TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations)
                    || (Settings.Default.KeySelectionTriggerIncompleteFixationTtl != TimeSpan.FromMilliseconds(KeySelectionTriggerIncompleteFixationTtlInMs) && KeySelectionTriggerSource == Enums.TriggerSources.Fixations);
            }
        }

        #endregion

        #region Methods

        private void Load()
        {
            PointsSource = Settings.Default.PointsSource;
            TobiiEyeXProcessingLevel = Settings.Default.TobiiEyeXProcessingLevel;
            PointsMousePositionSampleIntervalInMs = Settings.Default.PointsMousePositionSampleInterval.TotalMilliseconds;
            PointTtlInMs = Settings.Default.PointTtl.TotalMilliseconds;
            KeySelectionTriggerSource = Settings.Default.KeySelectionTriggerSource;
            KeySelectionTriggerKeyboardKeyDownUpKey = Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey;
            KeySelectionTriggerMouseDownUpButton = Settings.Default.KeySelectionTriggerMouseDownUpButton;
            KeySelectionTriggerFixationLockOnTimeInMs = Settings.Default.KeySelectionTriggerFixationLockOnTime.TotalMilliseconds;
            KeySelectionTriggerFixationResumeRequiresLockOn = Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn;
            KeySelectionTriggerFixationCompleteTimeInMs = Settings.Default.KeySelectionTriggerFixationCompleteTime.TotalMilliseconds;
            KeySelectionTriggerIncompleteFixationTtlInMs = Settings.Default.KeySelectionTriggerIncompleteFixationTtl.TotalMilliseconds;
            ProgressIndicatorBehaviour = Settings.Default.ProgressIndicatorBehaviour;
            ProgressIndicatorResizeStartProportion = Settings.Default.ProgressIndicatorResizeStartProportion;
            ProgressIndicatorResizeEndProportion = Settings.Default.ProgressIndicatorResizeEndProportion;
        }

        public void ApplyChanges()
        {
            Settings.Default.PointsSource = PointsSource;
            Settings.Default.TobiiEyeXProcessingLevel = TobiiEyeXProcessingLevel;
            Settings.Default.PointsMousePositionSampleInterval = TimeSpan.FromMilliseconds(PointsMousePositionSampleIntervalInMs);
            Settings.Default.PointTtl = TimeSpan.FromMilliseconds(PointTtlInMs);
            Settings.Default.KeySelectionTriggerSource = KeySelectionTriggerSource;
            Settings.Default.KeySelectionTriggerKeyboardKeyDownUpKey = KeySelectionTriggerKeyboardKeyDownUpKey;
            Settings.Default.KeySelectionTriggerMouseDownUpButton = KeySelectionTriggerMouseDownUpButton;
            Settings.Default.KeySelectionTriggerFixationLockOnTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationLockOnTimeInMs);
            Settings.Default.KeySelectionTriggerFixationResumeRequiresLockOn = KeySelectionTriggerFixationResumeRequiresLockOn;
            Settings.Default.KeySelectionTriggerFixationCompleteTime = TimeSpan.FromMilliseconds(KeySelectionTriggerFixationCompleteTimeInMs);
            Settings.Default.ProgressIndicatorBehaviour = ProgressIndicatorBehaviour;
            Settings.Default.ProgressIndicatorResizeStartProportion = ProgressIndicatorResizeStartProportion;
            Settings.Default.ProgressIndicatorResizeEndProportion = ProgressIndicatorResizeEndProportion;
        }

        #endregion
    }
}
