using System;

namespace GLGraph.NET {
    public class GLRect {
        public GLPoint Location { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public GLRect(double x, double y, double width, double height) {
            Location = new GLPoint(x, y);
            Width = width;
            Height = height;
        }

        public GLRect(GLPoint point1, GLPoint point2) {
            Location = new GLPoint(Math.Min(point1.X, point2.X),Math.Min(point1.Y, point2.Y));
            //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0) 
            Width = Math.Max(Math.Max(point1.X, point2.X) - X, 0);
            Height = Math.Max(Math.Max(point1.Y, point2.Y) - Y, 0);
        }

        public double X { get { return Location.X; } }
        public double Y { get { return Location.Y; } }

        public bool Contains(double x, double y) {
            return ((x >= X) && (x - Width <= X) &&
                    (y >= Y) && (y - Height <= Y));
        }

        public bool ContainsX(double x) {
            return (x >= X) && (x - Width <= X);
        }
        
        public bool ContainsY(double y) {
            return (y >= Y) && (y - Height <= Y);
        }
    }
}
