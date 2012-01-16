using System;

namespace GLGraph.NET {
    public interface IDrawable {
        void Draw(GraphWindow window);
        bool Visible { get; set; }
    }

}
