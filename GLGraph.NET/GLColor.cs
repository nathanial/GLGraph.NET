using SharpGL;

namespace GLGraph.NET {
    public class GLColor {
        readonly double _a, _r, _g, _b;
        public double A { get { return _a; } }
        public double R { get { return _r; } }
        public double G { get { return _g; } }
        public double B { get { return _b; } }

        public GLColor(double a, double r, double g, double b) {
            _a = a;
            _r = r;
            _g = g;
            _b = b;
        }

        public void Draw(OpenGL gl) {
            gl.Color(_r, _g, _b, _a);
        }

        public void DrawFaded(OpenGL gl) {
            gl.Color(_r,_g,_b,0.1);
        }
    }
}