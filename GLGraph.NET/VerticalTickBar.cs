using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OpenTK.Graphics.OpenGL;
using Point = System.Windows.Point;

namespace GLGraph.NET {

    public interface ITickBar : IDisposable {
        double TickStart { get; set; }
        double RangeStop { get; set; }
        double MinorTick { get; set; }
        double MajorTick { get; set; }
        double RangeStart { get; set; }
        GraphWindow Window { get; set; }
        void Draw();
        void DrawCrossLines();
    }

    public class VerticalTickBar : ITickBar {
        public double TickStart { get; set; }
        public double RangeStop { get; set; }

        public double RangeStart { get; set; }

        public double MinorTick { get; set; }
        public double MajorTick { get; set; }
        public GraphWindow Window { get; set; }

        readonly IList<PieceOfText> _texts = new List<PieceOfText>();
        readonly Font _font = new Font("Arial", 10);

        public void Draw() {
            foreach (var t in _texts) {
                t.Dispose();
            }
            _texts.Clear();

            GL.PushMatrix();
            
            MoveFiftyPixelsRight();

            GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.DataHeight, 1);
            GL.Translate(0, -Window.DataOrigin.Y, 0);

            GL.Color3(0.0, 0.0, 0.0);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    DrawMajorTick(TickStart + i);
                } else if (Math.Abs(i % MinorTick) < 0.0001) {
                    DrawMinorTick(TickStart + i);
                }
            }
            GL.End();

            GL.PopMatrix();

            GL.PushMatrix();
            MoveFiftyPixelsRight();
            GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1.0);

            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    var t = new PieceOfText(_font, i.ToString(CultureInfo.InvariantCulture));
                    t.Draw(new Point(0, ((i - Window.Bottom) / Window.DataHeight) * Window.WindowHeight - 17));
                    _texts.Add(t);
                }
            }
            GL.PopMatrix();
        }

        public void DrawCrossLines() {
            GL.PushMatrix();
            
            MoveFiftyPixelsRight();

            GL.Scale(1.0/Window.WindowWidth, 1.0/Window.DataHeight,1);
            GL.Translate(50, -Window.DataOrigin.Y, 0);

            GL.Color4(0.0, 0.0, 0.0, 0.25);
            GL.LineWidth(0.5f);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    GL.Vertex2(0, TickStart + i);
                    GL.Vertex2(Window.WindowWidth - 50, TickStart + i);
                }
            }
            GL.End();
            GL.LineWidth(1.0f);

            GL.PopMatrix();
        }

        void MoveFiftyPixelsRight() {
            GL.Scale(1.0/Window.WindowWidth, 1.0/Window.WindowHeight, 1.0);
            GL.Translate(0, 50, 0);
            GL.Scale(Window.WindowWidth, Window.WindowHeight, 1.0);
        }

        public void Dispose() {
        }

        void DrawMajorTick(double i) {
            GL.Vertex2(30, i);
            GL.Vertex2(50, i);
        }

        void DrawMinorTick(double i) {
            GL.Vertex2(40, i);
            GL.Vertex2(50, i);
        }
    }
}
