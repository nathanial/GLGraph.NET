using OpenTK.Graphics.OpenGL;

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

        public void Draw() {
            GL.Color4(_r, _g, _b, _a);
        }

        public void DrawFaded() {
            GL.Color4(_r,_g,_b,0.1);
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            var c = obj as GLColor;
            if (c == null) return false;
            return _a == c._a &&
                   _r == c._r &&
                   _g == c._g &&
                   _b == c._b;
        }

        public override int GetHashCode() {
            return _a.GetHashCode() ^ _r.GetHashCode() ^ _g.GetHashCode() ^ _b.GetHashCode();
        }
    }
}