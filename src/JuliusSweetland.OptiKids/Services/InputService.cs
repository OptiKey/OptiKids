using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Observables.PointSources;
using JuliusSweetland.OptiKids.Observables.TriggerSources;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.Services
{
    public partial class InputService : BindableBase, IInputService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IPointSource pointSource;
        private readonly ITriggerSource keySelectionTriggerSource;
        private readonly object suspendRequestLock = new object();

        private int suspendRequestCount;
        
        private event EventHandler<Tuple<Point, KeyValue?>> currentPositionEvent;
        private event EventHandler<Tuple<PointAndKeyValue?, double>> selectionProgressEvent;
        private event EventHandler<PointAndKeyValue> selectionEvent;

        #endregion

        #region Ctor

        public InputService(
            IPointSource pointSource,
            ITriggerSource keySelectionTriggerSource)
        {
            this.pointSource = pointSource;
            this.keySelectionTriggerSource = keySelectionTriggerSource;
        }

        #endregion

        #region Properties
        
        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set
            {
                Log.DebugFormat("PointToKeyValueMap property changed (setter called with {0} map).", value != null ? "non-null" : "null");

                if (pointSource != null)
                {
                    pointSource.PointToKeyValueMap = value;
                }
            }
        }

        #endregion

        #region Events
        
        #region Current Position

        public event EventHandler<Tuple<Point, KeyValue?>> CurrentPosition
        {
            add
            {
                if (currentPositionEvent == null)
                {
                    Log.Info("CurrentPosition event has first subscriber.");
                }

                currentPositionEvent += value;

                if (currentPositionSubscription == null)
                {
                    CreateCurrentPositionSubscription();
                }
            }
            remove
            {
                currentPositionEvent -= value;
            }
        }

        #endregion

        #region Selection Progress

        public event EventHandler<Tuple<PointAndKeyValue?, double>> SelectionProgress
        {
            add
            {
                if (selectionProgressEvent == null)
                {
                    Log.Info("SelectionProgress event has first subscriber.");
                }

                selectionProgressEvent += value;
                
                if (selectionProgressSubscription == null)
                {
                    CreateSelectionProgressSubscription();
                }
            }
            remove
            {
                selectionProgressEvent -= value;
                
                if (selectionProgressEvent == null)
                {
                    Log.Info("Last subscriber of SelectionProgress event has unsubscribed. Disposing of selectionProgressSubscription.");
                    
                    if (selectionProgressSubscription != null)
                    {
                        selectionProgressSubscription.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Selection

        public event EventHandler<PointAndKeyValue> Selection
        {
            add
            {
                if (selectionEvent == null)
                {
                    Log.Info("Selection event has first subscriber.");
                }

                selectionEvent += value;

                if (selectionTriggerSubscription == null)
                {
                    CreateSelectionSubscriptions();
                }
            }
            remove
            {
                selectionEvent -= value;

                if (selectionEvent == null)
                {
                    Log.Info("Last subscriber of Selection event has unsubscribed.");
                }
            }
        }

        #endregion

        public event EventHandler<Exception> Error;

        #endregion

        #region Publish Events

        #region Publish Current Position

        private void PublishCurrentPosition(Tuple<Point, KeyValue?> currentPosition)
        {
            if (currentPositionEvent != null)
            {
                Log.DebugFormat("Publishing CurrentPosition event with Point:{0} KeyValue:{1}", currentPosition.Item1, currentPosition.Item2);

                currentPositionEvent(this, currentPosition);
            }
        }

        #endregion

        #region Publish Selection Progress

        private void PublishSelectionProgress(Tuple<PointAndKeyValue?, double> selectionProgress)
        {
            if (selectionProgressEvent != null)
            {
                if ((selectionProgress.Item2 < 0.1) || (selectionProgress.Item2 - 0.5) < 0.1 || (selectionProgress.Item2 - 1) < 0.1)
                {
                    Log.DebugFormat("Publishing SelectionProgress event: {0} : {1}", selectionProgress.Item1, selectionProgress.Item2);
                }

                selectionProgressEvent(this, selectionProgress);
            }
        }

        #endregion

        #region Publish Selection

        private void PublishSelection(PointAndKeyValue selection)
        {
            if (selectionEvent != null)
            {
                Log.DebugFormat("Publishing Selection event with PointAndKeyValue:{0}", selection);

                selectionEvent(this, selection);
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion

        #endregion

        #region Methods

        public void RequestSuspend()
        {
            Log.InfoFormat("RequestSuspend received. SuspendRequestCount={0} before it is incremented.", suspendRequestCount);
            lock (suspendRequestLock)
            {
                suspendRequestCount++;
                if (keySelectionTriggerSource.State == RunningStates.Running)
                {
                    keySelectionTriggerSource.State = RunningStates.Paused;
                }
                if (pointSource.State == RunningStates.Running)
                {
                    pointSource.State = RunningStates.Paused;
                }
            }
        }

        public void RequestResume()
        {
            Log.InfoFormat("RequestResume received. SuspendRequestCount={0} before it is decremented.", suspendRequestCount);
            lock (suspendRequestLock)
            {
                suspendRequestCount--;
                if (suspendRequestCount == 0)
                {
                    if (keySelectionTriggerSource != null)
                    {
                        keySelectionTriggerSource.State = RunningStates.Running;
                    }
                    if (pointSource != null)
                    {
                        pointSource.State = RunningStates.Running;
                    }
                }
            }

            if (suspendRequestCount < 0)
            {
                Log.WarnFormat("InputService suspend request counter is below zero. Current value:{0}", suspendRequestCount);
            }
        }

        #endregion
    }
}
