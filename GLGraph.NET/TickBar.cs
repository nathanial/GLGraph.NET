using System;
using System.Diagnostics;
using System.Windows;
using OpenTK.Graphics.OpenGL;

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

        public void Draw() {
            GL.Color3(0.0, 0.0, 0.0);
            GL.Begin(BeginMode.Lines);
            for (var i = RangeStart; i < RangeStop; i++) {
                if (Math.Abs(i % MajorTick) < 0.0001) {
                    DrawMajorTick(TickStart + i);
                } else if(Math.Abs(i % MinorTick) < 0.0001) {
                    DrawMinorTick(TickStart + i);
                }
            }
            GL.End();
        }

        void DrawMajorTick(double i) {
            GL.Vertex2(0, i);
            GL.Vertex2(1, i);
        }

        void DrawMinorTick(double i) {
            GL.Vertex2(0.5,i);
            GL.Vertex2(1,i);
        }

        public void DrawCrossLines() {
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
