using System;
using System.Collections.Generic;

namespace GLGraph.NET {
    public static class RangeHelper {
        public static IList<double> FindTicks(double interval, double start, double stop) {
            var ticks = new List<double>();
            double? firstTick = null;
            var distance = start%interval;
            firstTick = start - distance;
            //for (var i = start; i < stop; i++) {
            //    if (Math.Abs(i % interval) < 0.0001) {
            //        firstTick = i;
            //        break;
            //    }
            //}
            //if (firstTick.HasValue) {
                for (var i = firstTick.Value; i < stop; i += interval) {
                    ticks.Add(i);
                }
            //}
            return ticks;
        }
    }
}