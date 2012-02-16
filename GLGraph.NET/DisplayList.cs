using System;
using SharpGL;

namespace GLGraph.NET {

    public class DisplayList {
        readonly uint _id;

        public DisplayList(OpenGL gl, Action listInstructions) {
            _id = Pools.DisplayListPool.Take();
            gl.NewList(_id, OpenGL.GL_COMPILE);
            listInstructions();
            gl.EndList();
        }

        public void Draw(OpenGL gl) {
            gl.CallList(_id);
        }

        public void Dispose(OpenGL gl) {
            gl.DeleteLists(_id, 1);
            Pools.DisplayListPool.Return(_id);
        }
    }
}