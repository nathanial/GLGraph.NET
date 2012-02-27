using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
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

            GL.LoadIdentity();
            GL.Ortho(0, Window.WindowWidth, 0, Window.DataHeight, -1, 1);
            GL.Translate(10, Window.DataHeight / 10.0, 0);
            GL.Scale(1, 1, 0);
            GL.Translate(0, -Window.DataOrigin.Y, 0);

            GL.Color3(0.0, 0.0, 0.0);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    DrawMajorTick(TickStart + i);
                    var t = new PieceOfText(_font, i.ToString(CultureInfo.InvariantCulture));
                    //t.Draw(Window, new Point(0, TickStart + i));
                    _texts.Add(t);

                } else if(Math.Abs(i % MinorTick) < 0.0001) {
                    DrawMinorTick(TickStart + i);
                }
            }
            GL.End();

        }

        public void DrawCrossLines() {
            GL.LoadIdentity();
            GL.Ortho(0, Window.WindowWidth, 0, 1000, -1, 1);
            GL.Translate(40, 100, 0);
            GL.Scale(Window.WindowWidth, 1000 / Window.DataHeight, 0);
            GL.Translate(0, -Window.DataOrigin.Y, 0);

            GL.Color4(0.0, 0.0, 0.0,0.25);
            GL.LineWidth(0.5f);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    DrawMajorTick(TickStart + i);
                }
            }
            GL.End();
            GL.LineWidth(1.0f);
        }

        public void Dispose() {
        }

        void DrawMajorTick(double i) {
            GL.Vertex2(0, i);
            GL.Vertex2(30, i);
        }

        void DrawMinorTick(double i) {
            GL.Vertex2(15, i);
            GL.Vertex2(30, i);
        }
    }

    public class HorizontalTickBar : ITickBar {
        public double RangeStart { get; set; }
        public double TickStart { get; set; }
        public double RangeStop { get; set; }
        public double MinorTick { get; set; }
        public double MajorTick { get; set; }
        public GraphWindow Window { get; set; }

        public void Draw() {
        }

        public void DrawCrossLines() {
        }

        public void Dispose() {
        }

    }



}
