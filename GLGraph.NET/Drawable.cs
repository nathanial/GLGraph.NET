using System;

namespace GLGraph.NET {
    public interface IDrawable : IDisposable {
        void Draw(GraphWindow window);
    }

}
