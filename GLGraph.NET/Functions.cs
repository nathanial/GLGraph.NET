using System;
using System.Collections.Generic;

namespace GLGraph.NET {
    public static class Functions {
        public static int FindFirst(int start, int finish, Func<int, bool> predicate) {
            for (var i = start; i < finish; i++) {
                if (predicate(i)) {
                    return i;
                }
            }
            return start;
        }
        public static void LoopOver(int start, int finish, int step, Action<int> action) {
            for (var i = start; i < finish; i += step) {
                action(i);
            }
        }

        public static T[] SelectOverMany<T>(int start, int finish, int step, Func<int, T[]> func) {
            var results = new List<T>();
            for (var i = start; i < finish; i += step) {
                results.AddRange(func(i));
            }
            return results.ToArray();
        }

        public static T[] SelectOver<T>(int start, int finish, int step, Func<int, T> func) {
            var results = new List<T>();
            for (var i = start; i < finish; i += step) {
                results.Add(func(i));
            }
            return results.ToArray();
        }
    }
}