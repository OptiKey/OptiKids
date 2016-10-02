using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.Services
{
    public partial class InputService
    {
        #region Fields
        
        private IDisposable currentPositionSubscription;
        private IDisposable selectionProgressSubscription;
        private IDisposable selectionTriggerSubscription;

        #endregion
        

        #region Create Current Position Subscription

        private void CreateCurrentPositionSubscription()
        {
            Log.Debug("Creating subscription to PointAndKeyValueSource for current position.");

            currentPositionSubscription = pointSource.Sequence
                .Where(tp => tp.Value != null)
                .Select(tp => new Tuple<Point, KeyValue?>(
                    tp.Value.Value.Point, tp.Value.Value.KeyValue))
                .DistinctUntilChanged()
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishCurrentPosition);
        }

        #endregion

        #region Create Selection Progress Subscription

        private void CreateSelectionProgressSubscription()
        {
            Log.DebugFormat("Creating subscription to Key SelectionTriggerSource for progress info.");

            if (keySelectionTriggerSource != null)
            {
                selectionProgressSubscription = keySelectionTriggerSource.Sequence
                    .Where(ts => ts.Progress != null)
                    .DistinctUntilChanged()
                    .ObserveOnDispatcher()
                    .Subscribe(ts =>
                    {
                        PublishSelectionProgress(new Tuple<PointAndKeyValue?, double>(ts.PointAndKeyValue, ts.Progress.Value));
                    });
            }
        }

        #endregion

        #region Create Selection Subscriptions

        private void CreateSelectionSubscriptions()
        {
            Log.Debug("Creating subscription to KeySelectionTriggerSource for selections & results.");

            if (keySelectionTriggerSource != null)
            {
                selectionTriggerSubscription = keySelectionTriggerSource.Sequence
                    .ObserveOnDispatcher()
                    .Subscribe(ProcessSelectionTrigger);
            }
        }

        private void ProcessSelectionTrigger(TriggerSignal triggerSignal)
        {
            if (triggerSignal.Signal >= 1)
            {
                if (triggerSignal.PointAndKeyValue != null)
                {
                    Log.Debug("Selection trigger signal (with PointAndKeyValue) detected.");

                    if (triggerSignal.PointAndKeyValue.Value.KeyValue != null)
                    {
                        PublishSelection(triggerSignal.PointAndKeyValue.Value);
                    }
                    else
                    {
                        Log.Debug("The trigger occurred away from a key.");
                    }
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    PublishError(this, new ApplicationException(Resources.TRIGGER_WITHOUT_POSITION_ERROR));
                }
            }
        }

        #endregion

        #region Dispose Selection Subscriptions

        private void DisposeSelectionSubscriptions()
        {
            Log.Debug("Disposing of subscriptions to SelectionTriggerSource for selections & results.");

            if (selectionTriggerSubscription != null)
            {
                selectionTriggerSubscription.Dispose();
            }
        }

        #endregion
    }
}
