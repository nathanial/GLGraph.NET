using System;
using System.Windows;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {

    public enum TickBarOrientation {
        Vertical,
        Horizontal
    }

    public class TickBar {
        public int MinorTick { get; set; }
        public int MajorTick { get; set; }

        GDIOpenGLTextRenderer _textRenderer;
        readonly TickBarOrientation _orientation;
        readonly GLColor _white = new GLColor(1.0f, 1.0f, 1.0f, 1.0f);

        ILineGraph _graph;

        public TickBar(ILineGraph graph, TickBarOrientation orientation) {
            _graph = graph;
            _orientation = orientation;
        }

        public void Draw(GraphWindow window) {
            _textRenderer = _textRenderer ?? new GDIOpenGLTextRenderer();
            Point origin;
            GLSize size;
            Func<int, int, Point[]> line;
            Func<int, int> start;
            int end;
            Func<int, PieceOfText> labelFunc;
            var major = AdjustedMajorTick(window);
            var minor = AdjustedMinorTick(window);

            if (_orientation == TickBarOrientation.Vertical) {
                origin = new Point(0, _graph.BottomMargin);
                size = new GLSize(_graph.LeftMargin, window.WindowHeight - _graph.BottomMargin);
                start = t => VerticalStart(window, t);
                end = (int)window.Top;
                line = (l, i) => CreateVerticalLine(window, i, (int)_graph.LeftMargin, l);
                labelFunc = i => new PieceOfText(0, new Point(0, i + _graph.YOffset).ToScreen(window).Y - _graph.BottomMargin + 7.5, i.ToString());
            } else {
                origin = new Point(_graph.LeftMargin, 0);
                size = new GLSize(window.WindowWidth - _graph.LeftMargin, _graph.BottomMargin);
                start = t => HorizontalStart(window, t);
                end = (int)window.Finish;
                line = (l, i) => CreateHorizontalLine(window, i, (int)_graph.BottomMargin, l);
                labelFunc = i => new PieceOfText(new Point(i + _graph.XOffset, 0).ToScreen(window).X - _graph.LeftMargin - ((i.ToString().Length / 2.0) * 8), 30, i.ToString());
            }

            var labels = Functions.SelectOver(start(major), end, major, labelFunc);
            var rect = new GLRectangle(_white, true, origin, size);
            rect.Draw();
            _textRenderer.AddText(labels);
            _textRenderer.Draw(WidthForOrientation(window), HeightForOrientation(window), rect);

            if (Math.Abs(window.WindowWidth) >= 0.001 && Math.Abs(window.WindowHeight) >= 0.001) {
                GL.Color3(0.0f, 0.0f, 0.0f);
                GL.Begin(BeginMode.Lines);
                double margin = _orientation == TickBarOrientation.Vertical ? _graph.LeftMargin : _graph.BottomMargin;
                OpenGL.DrawMany(start(major), end, major, i => line((int) (margin - 20), i));
                OpenGL.DrawMany(start(minor), end, minor, i => i % major == 0 ? new Point[] { } : line((int) (margin - 10), i));
                GL.End();
            }
        }

        int AdjustMajorTick(GraphWindow window, int majorTick) {
            var count = _orientation == TickBarOrientation.Vertical
                            ? VerticalTickCount(window, majorTick)
                            : HorizontalTickCount(window, majorTick);
            if (count > 20) {
                majorTick *= (count / 20);
            }
            return majorTick;
        }

        int AdjustMinorTick(GraphWindow window, int majorTick, int minorTick) {
            var count = _orientation == TickBarOrientation.Vertical
                            ? VerticalTickCount(window, majorTick)
                            : HorizontalTickCount(window, majorTick);
            if (count > 20) {
                minorTick *= (count / 20);
            }
            return minorTick;
        }

        int WidthForOrientation(GraphWindow window) {
            return (int)(_orientation == TickBarOrientation.Horizontal ? window.WindowWidth - _graph.LeftMargin : _graph.LeftMargin);
        }

        int HeightForOrientation(GraphWindow window) {
            return (int)(_orientation == TickBarOrientation.Vertical ? window.WindowHeight - _graph.BottomMargin : _graph.BottomMargin);
        }

        public int AdjustedMajorTick(GraphWindow window) {
            return AdjustMajorTick(window, MajorTick);
        }

        public int AdjustedMinorTick(GraphWindow window) {
            return AdjustMinorTick(window, MajorTick, MinorTick);
        }

        public static int HorizontalStart(GraphWindow window, int tick) {
            return Functions.FindFirst((int)window.Start, (int)window.Finish, i => i % tick == 0);
        }

        public static int VerticalStart(GraphWindow window, int tick) {
            return Functions.FindFirst((int)window.Bottom, (int)window.Top, i => i % tick == 0);
        }

        static int VerticalTickCount(GraphWindow window, int majorTick) {
            return ((int)window.Top - (int)window.Bottom) / majorTick;
        }

        static int HorizontalTickCount(GraphWindow window, int majorTick) {
            return ((int)window.Finish - (int)window.Start) / majorTick;
        }

        Point[] CreateHorizontalLine(GraphWindow window, int i, int y1, int y2) {
            var r = new Point(i + _graph.XOffset, 0).ToScreen(window);
            return new[] {
                new Point(r.X, y1),
                new Point(r.X, y2)  
            };
        }

        Point[] CreateVerticalLine(GraphWindow window, int i, int x1, int x2) {
            var r = new Point(0, i + _graph.YOffset).ToScreen(window);
            return new[] {
                new Point(x1, r.Y),
                new Point(x2, r.Y)
            };
        }

        public void Dispose() {
            _textRenderer.Dispose();
        }
    }

    public static class PointExtensions {
        public static Point ToView(this Point p, GraphWindow w) {
            var xscale = (w.Finish - w.Start) / w.WindowWidth;
            var yscale = (w.Top - w.Bottom) / w.WindowHeight;
            const double xoffset = 0; //window x start
            const double yoffset = 0; //window y start
            return new Point((p.X - xoffset) * xscale,
                             (p.Y - yoffset) * yscale);
        }

        public static Point ToScreen(this Point p, GraphWindow w) {
            var xscale = w.WindowWidth / (w.Finish - w.Start);
            var yscale = w.WindowHeight / (w.Top - w.Bottom);
            var xoffset = w.Start;
            var yoffset = w.Bottom;
            return new Point((p.X - xoffset) * xscale,
                             (p.Y - yoffset) * yscale);
        }
    }
}
