using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using OpenTK.Graphics.OpenGL;
using Point = System.Windows.Point;

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

            OpenGL.PushMatrix(() => {
                GL.Color3(1.0, 1.0, 1.0);
                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1);
                OpenGL.Begin(BeginMode.Quads, () => {
                    GL.Vertex2(0,50);
                    GL.Vertex2(Window.WindowWidth,50);
                    GL.Vertex2(Window.WindowWidth,0);
                    GL.Vertex2(0,0);
                });
            });

            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsRight();

                GL.Scale(1.0 / Window.DataWidth, 1.0 / Window.WindowHeight, 1);
                GL.Translate(-Window.DataOrigin.X, 0, 0);

                GL.Color3(0.0, 0.0, 0.0);
                OpenGL.Begin(BeginMode.Lines, () => {
                    for (var i = RangeStart; i < RangeStop; i++) {
                        if (Math.Abs(i % MajorTick) < 0.0001) {
                            DrawMajorTick(TickStart + i);
                        } else if (Math.Abs(i % MinorTick) < 0.0001) {
                            DrawMinorTick(TickStart + i);
                        }
                    }
                });
            });

            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsRight();
                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1.0);

                for (var i = RangeStart; i < RangeStop; i++) {
                    if (Math.Abs(i % MajorTick) < 0.0001) {
                        var t = new PieceOfText(_font, i.ToString(CultureInfo.InvariantCulture));
                        t.Draw(new Point(((i - Window.Start) / Window.DataWidth) * Window.WindowWidth - 5, 0));
                        _texts.Add(t);
                    }
                }
            });
        }

        public void DrawCrossLines() {
            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsRight();
                GL.Scale(1.0 / Window.DataWidth, 1.0 / Window.WindowHeight, 1);
                GL.Translate(-Window.DataOrigin.X, 50, 0);

                GL.Color4(0.0, 0.0, 0.0, 0.25);
                GL.LineWidth(0.5f);
                OpenGL.Begin(BeginMode.Lines, () => {
                    for (var i = RangeStart; i < RangeStop; i++) {
                        if (Math.Abs(i % MajorTick) < 0.0001) {
                            GL.Vertex2(TickStart + i, 0);
                            GL.Vertex2(TickStart + i, Window.WindowHeight);
                        }
                    }
                });
                GL.LineWidth(1.0f);
            });
        }

        public void Dispose() {
        }

        void DrawMajorTick(double x) {
            GL.Vertex2(x, 50);
            GL.Vertex2(x, 30);
        }

        void DrawMinorTick(double x) {
            GL.Vertex2(x, 50);
            GL.Vertex2(x, 40);
        }

        void MoveFiftyPixelsRight() {
            GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1.0);
            GL.Translate(50, 0, 0);
            GL.Scale(Window.WindowWidth, Window.WindowHeight, 1.0);
        }

    }
}