using System;
using System.Collections.Generic;

namespace GLGraph.NET {

    public class IntegerPool {
        readonly SortedSet<int> _pool = new SortedSet<int>();

        public IntegerPool(int start, int stop) {
            for (var i = start; i < stop; i++) {
                _pool.Add(i);
            }
        }

        public int Take() {
            if (_pool.Count == 0) throw new Exception("pool is empty");
            var i = _pool.Min;
            _pool.Remove(i);
            return i;
        }

        public void Return(int i) {
            if (_pool.Contains(i)) throw new Exception("double return");
            _pool.Add(i);
        }
    }

    public static class Pools {
        public static readonly IntegerPool DisplayListPool = new IntegerPool(1, 10000);
    }
}
