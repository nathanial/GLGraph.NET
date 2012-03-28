using System;
using System.Windows;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {
    public class GLRectangle {
        public Point Origin { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public GLColor Color { get; set; }
        public bool Border { get; set; }
        public Size Size { get; set; }


        public GLRectangle(GLColor color, bool border, Point origin, Size size) {
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

        public Point TopLeft {
            get { return new Point(Origin.X, Origin.Y + Size.Height); }
        }

        public Point TopRight {
            get { return new Point(Origin.X + Size.Width, Origin.Y + Size.Height); }
        }

        public Point BottomRight {
            get { return new Point(Origin.X + Size.Width, Origin.Y); }
        }

        public Point BottomLeft {
            get { return new Point(Origin.X, Origin.Y); }
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