using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKids.Models;

namespace JuliusSweetland.OptiKids.Extensions
{
    public static class PointExtensions
    {
        /// <summary>
        /// Convert a point to a PointAndKeyValue (if one can be mapped from the supplied pointToKeyValueMap).
        /// N.B. Null will be returned if the point supplied is null.
        /// </summary>
        public static PointAndKeyValue? ToPointAndKeyValue(this Point? point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            if (point == null)
            {
                return null;
            }

            return new PointAndKeyValue(point.Value, point.Value.ToKeyValue(pointToKeyValueMap));
        }

        public static KeyValue? ToKeyValue(this Point point, Dictionary<Rect, KeyValue> pointToKeyValueMap)
        {
            Rect? keyRect = pointToKeyValueMap != null
                ? pointToKeyValueMap.Keys.FirstOrDefault(r => r.Contains(point))
                : (Rect?)null;

            return keyRect != null && pointToKeyValueMap.ContainsKey(keyRect.Value)
                ? pointToKeyValueMap[keyRect.Value]
                : (KeyValue?)null;
        }
    }
}
