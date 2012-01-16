using System;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {

    public class DisplayList : IDisposable {
        readonly int _id;

        public DisplayList(Action listInstructions) {
            _id = Pools.DisplayListPool.Take();
            GL.NewList(_id, ListMode.Compile);
            listInstructions();
            GL.EndList();
        }

        public void Draw() {
            GL.CallList(_id);
        }

        public void Dispose() {
            GL.DeleteLists(_id, 1);
            Pools.DisplayListPool.Return(_id);
        }
    }
}