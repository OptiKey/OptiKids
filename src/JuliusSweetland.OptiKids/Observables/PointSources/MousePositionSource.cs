using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Forms;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.Observables.PointSources
{
    public class MousePositionSource : IPointSource
    {
        private readonly TimeSpan pointTtl;

        private IObservable<Timestamped<PointAndKeyValue?>> sequence;

        public MousePositionSource(TimeSpan pointTtl)
        {
            this.pointTtl = pointTtl;
        }

        public RunningStates State { get; set; }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }

        public IObservable<Timestamped<PointAndKeyValue?>> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    sequence = Observable
                        .Interval(Settings.Default.PointsMousePositionSampleInterval)
                        .Where(_ => State == RunningStates.Running)
                        .Select(_ => new Point(Cursor.Position.X, Cursor.Position.Y))
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue?>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                }

                return sequence;
            }
        }
    }
}
