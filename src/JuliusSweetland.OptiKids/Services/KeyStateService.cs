using System.Reflection;
using JuliusSweetland.OptiKids.Models;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.Services
{
    public class KeyStateService : BindableBase, IKeyStateService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly NotifyingConcurrentDictionary<KeyValue, double> keySelectionProgress;
        
        private bool simulateKeyStrokes;
        private bool turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased;
        
        #endregion

        #region Ctor

        public KeyStateService()
        {
            keySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
        }

        #endregion

        #region Properties

        public NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get { return keySelectionProgress; } }

        #endregion
    }
}
