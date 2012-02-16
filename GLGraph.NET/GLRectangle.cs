using System.Windows;
using SharpGL;

namespace GLGraph.NET {
    public class GLRectangle {
        readonly Point _origin;
        readonly GLSize _size;
        readonly GLColor _color;
        readonly bool _border;

        public Point Origin { get { return _origin; } }
        public double Width { get { return _size.Width; } }
        public double Height { get { return _size.Height; } }
        public GLColor Color { get { return _color; } }
        public bool Border { get { return _border; } }

        public GLRectangle(GLColor color, bool border, Point origin, GLSize size) {
            _origin = origin;
            _border = border;
            _size = size;
            _color = color;
        }

        public void Draw(OpenGL gl) {
            _color.Draw(gl);
            DrawCore(gl);
        }

        public void DrawFaded(OpenGL gl) {
            _color.DrawFaded(gl);
            DrawCore(gl);
        }

        public Point TopLeft {
            get { return new Point(_origin.X, _origin.Y + _size.Height); }
        }

        public Point TopRight {
            get { return new Point(_origin.X + _size.Width, _origin.Y + _size.Height); }
        }

        public Point BottomRight {
            get { return new Point(_origin.X + _size.Width, _origin.Y); }
        }

        public Point BottomLeft {
            get { return new Point(_origin.X, _origin.Y); }
        }

        public Size Size {
            get { return new Size(Width, Height); }
        }

        void DrawCore(OpenGL gl) {
            gl.DrawQuad(
                TopLeft, TopRight,
                BottomRight, BottomLeft);

            gl.Color(0.0f, 0.0f, 0.0f);
            gl.DrawLines(
                _origin.X, _origin.Y,
                _origin.X, _origin.Y + _size.Height,
                _origin.X, _origin.Y + _size.Height,
                _origin.X + _size.Width, _origin.Y + _size.Height,
                _origin.X + _size.Width, _origin.Y + _size.Height,
                _origin.X + _size.Width, _origin.Y,
                _origin.X + _size.Width, _origin.Y,
                _origin.X, _origin.Y);
        }
    }
}