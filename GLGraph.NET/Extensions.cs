using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GLGraph.NET {

    public static class PointExtensions {
        public static Point ToView(this Point p, GraphWindow w) {
            var xscale = (w.Finish - w.Start) / w.WindowWidth;
            var yscale = (w.Top - w.Bottom) / w.WindowHeight;
            const double xoffset = 0; //window x start
            const double yoffset = 0; //window y start
            return new Point((p.X - xoffset) * xscale,
                             (p.Y - yoffset) * yscale);
        }

        public static Point ToScreen(this Point p, GraphWindow w) {
            var xscale = w.WindowWidth / (w.Finish - w.Start);
            var yscale = w.WindowHeight / (w.Top - w.Bottom);
            var xoffset = w.Start;
            var yoffset = w.Bottom;
            return new Point((p.X - xoffset) * xscale,
                             (p.Y - yoffset) * yscale);
        }

        public static double FromPixelsX(this GraphWindow window, double x) {
            return new Point(x, 0).ToView(window).X;
        }

        public static double FromPixelsY(this GraphWindow window, double y) {
            return new Point(0, y).ToView(window).Y;
        }
    }
}
