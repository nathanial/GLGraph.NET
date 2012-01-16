using System;

namespace GLGraph.NET {

    public class IntegerPool {
        readonly C5.TreeSet<int> _pool = new C5.TreeSet<int>();

        public IntegerPool(int start, int stop) {
            for (var i = start; i < stop; i++) {
                _pool.Add(i);
            }
        }

        public int Take() {
            if (_pool.IsEmpty) throw new Exception("pool is empty");
            var i = _pool.FindMin();
            _pool.Remove(i);
            return i;
        }

        public void Return(int i) {
            if (_pool.Contains(i)) throw new Exception("double return");
            _pool.Add(i);
        }
    }

    public static class Pools {
        public static readonly IntegerPool DisplayListPool = new IntegerPool(1, 1000);
        public static readonly IntegerPool TexturePool = new IntegerPool(1, 10);
    }
}
