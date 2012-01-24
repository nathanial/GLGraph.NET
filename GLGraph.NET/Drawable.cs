using System;
using System.Windows;

namespace GLGraph.NET {
    public interface IDrawable {
        void Draw(GraphWindow window);
        bool Visible { get; set; }
        Rect Dimensions { get; }
    }

}
