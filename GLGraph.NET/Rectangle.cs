
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {

    public class Rectangle {
        public GLPoint Origin { get; set; }
        public GLColor Color { get; set; }
        public bool Border { get; set; }
        public GLSize Size { get; set; }

        public Rectangle(GLColor color, bool border, GLPoint origin, GLSize size) {
            Origin = origin;
            Border = border;
            Size = size;
            Color = color;
        }

        public void Draw() {
            Color.Draw();
            DrawCore();
        }

        public void DrawFaded() {
            Color.DrawFaded();
            DrawCore();
        }

        public GLPoint TopLeft {
            get { return new GLPoint(Origin.X, Origin.Y + Size.Height); }
        }

        public GLPoint TopRight {
            get { return new GLPoint(Origin.X + Size.Width, Origin.Y + Size.Height); }
        }

        public GLPoint BottomRight {
            get { return new GLPoint(Origin.X + Size.Width, Origin.Y); }
        }

        public GLPoint BottomLeft {
            get { return new GLPoint(Origin.X, Origin.Y); }
        }

        void DrawCore() {
            OpenGL.DrawQuad(
                TopLeft, TopRight,
                BottomRight, BottomLeft);

            GL.Color3(0.0f, 0.0f, 0.0f);
            OpenGL.DrawLines(
                Origin.X, Origin.Y,
                Origin.X, Origin.Y + Size.Height,
                Origin.X, Origin.Y + Size.Height,
                Origin.X + Size.Width, Origin.Y + Size.Height,
                Origin.X + Size.Width, Origin.Y + Size.Height,
                Origin.X + Size.Width, Origin.Y,
                Origin.X + Size.Width, Origin.Y,
                Origin.X, Origin.Y);
        }
    }
}