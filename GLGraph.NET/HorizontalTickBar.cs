using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {
    public class HorizontalTickBar : ITickBar {
        public double RangeStart { get; set; }
        public double TickStart { get; set; }
        public double RangeStop { get; set; }
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
            GL.Translate(0.1, 0.0, 0.0);
            GL.Scale(1.0 / Window.DataWidth, 1.0 / Window.WindowHeight, 1);
            GL.Translate(-Window.DataOrigin.X, 0, 0);

            GL.Color3(0.0, 0.0, 0.0);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if(Math.Abs(i % MajorTick) < 0.0001) {
                    DrawMajorTick(TickStart + i);
                } else if(Math.Abs(i % MinorTick) < 0.0001) {
                    DrawMinorTick(TickStart + i);
                }
            }
            GL.End();

            GL.PopMatrix();

        }

        public void DrawCrossLines() {
        }

        public void Dispose() {
        }

        void DrawMajorTick(double x) {
            GL.Vertex2(x, 50);
            GL.Vertex2(x, 0);
        }

        void DrawMinorTick(double x) {
            GL.Vertex2(x, 50);
            GL.Vertex2(x, 25);
        }

    }
}