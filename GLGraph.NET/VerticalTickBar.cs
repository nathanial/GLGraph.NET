using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {

    public interface ITickBar : IDisposable {
        double TickStart { get; set; }
        double RangeStop { get; set; }
        double MinorTick { get; set; }
        double MajorTick { get; set; }
        double RangeStart { get; set; }
        GraphWindow Window { get; set; }
        void DrawTicks();
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

        public void DrawTicks() {
            foreach (var t in _texts) {
                t.Dispose();
            }
            _texts.Clear();

            DrawBackground();
            DrawMajorMinorTicks();
            DrawText();
        }

        void DrawText() {
            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsUp();
                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1.0);

                foreach (var tick in RangeHelper.FindTicks(MajorTick, RangeStart, RangeStop)) {
                    var t = new PieceOfText(_font, tick.ToString(CultureInfo.InvariantCulture));
                    t.Draw(new GLPoint(0, ((tick - Window.Bottom) / Window.DataHeight) * Window.WindowHeight), null, null, true);
                    _texts.Add(t);
                }
            });
        }

        void DrawMajorMinorTicks() {
            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsUp();

                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.DataHeight, 1);
                GL.Translate(0, -Window.DataOrigin.Y, 0);

                GL.Color3(0.0, 0.0, 0.0);
                GL.LineWidth(1.0f);
                GL.Disable(EnableCap.LineSmooth);
                OpenGL.Begin(BeginMode.Lines, () => {
                    var minorTicks = RangeHelper.FindTicks(MinorTick, RangeStart, RangeStop);
                    var majorTicks = RangeHelper.FindTicks(MajorTick, RangeStart, RangeStop);

                    foreach (var tick in minorTicks.Where(x => !majorTicks.Any(y => Math.Abs(x - y) < 0.001))) {
                        DrawMinorTick(TickStart + tick);
                    }
                    foreach (var tick in majorTicks) {
                        DrawMajorTick(TickStart + tick);
                    }
                });
                GL.Enable(EnableCap.LineSmooth);
            });
        }

        void DrawBackground() {
            OpenGL.PushMatrix(() => {
                //MoveFiftyPixelsUp();
                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1);

                GL.Color3(1.0, 1.0, 1.0);
                GL.LineWidth(1.0f);
                GL.Disable(EnableCap.LineSmooth);
                OpenGL.Begin(BeginMode.Quads, () => {
                    GL.Vertex2(0, Window.WindowHeight);
                    GL.Vertex2(50, Window.WindowHeight);
                    GL.Vertex2(50, 0);
                    GL.Vertex2(0, 0);
                });

                GL.Color3(0.0, 0.0, 0.0);
                GL.LineWidth(1.0f);
                OpenGL.Begin(BeginMode.Lines, () => {
                    GL.Vertex2(50, 50);
                    GL.Vertex2(50, Window.WindowHeight);
                });
                GL.Enable(EnableCap.LineSmooth);
            });
        }

        public void DrawCrossLines() {
            OpenGL.PushMatrix(() => {
                MoveFiftyPixelsUp();

                GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.DataHeight, 1);
                GL.Translate(50, -Window.DataOrigin.Y, 0);

                GL.Color4(0.0, 0.0, 0.0, 0.25);
                GL.LineWidth(0.5f);
                OpenGL.Begin(BeginMode.Lines, () => {
                    var majorTicks = RangeHelper.FindTicks(MajorTick, RangeStart, RangeStop);
                    foreach (var tick in majorTicks) {
                        GL.Vertex2(0, TickStart + tick);
                        GL.Vertex2(Window.WindowWidth - 50, TickStart + tick);
                    }
                });
                GL.LineWidth(1.0f);
            });
        }


        public void Dispose() {
            foreach (var t in _texts) {
                t.Dispose();
            }
            _texts.Clear();
            _font.Dispose();
        }

        void MoveFiftyPixelsUp() {
            GL.Scale(1.0 / Window.WindowWidth, 1.0 / Window.WindowHeight, 1.0);
            GL.Translate(0, 50, 0);
            GL.Scale(Window.WindowWidth, Window.WindowHeight, 1.0);
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
