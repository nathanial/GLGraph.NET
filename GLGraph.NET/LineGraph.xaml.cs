using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {
    public partial class LineGraph : ILineGraph  {
        readonly IList<IDrawable> _markers = new List<IDrawable>();
        bool _panningStarted;
        TickBar _leftTickBar;
        TickBar _bottomTickBar;
        int _xstart, _ystart;
        IList<Line> _lines;

        readonly IDictionary<Line, DisplayList> _displayLists = new Dictionary<Line, DisplayList>();

        GraphWindow Window { get; set; }

        public IDrawable[] Markers { get { return _markers.ToArray(); } }

        GLControl _glcontrol;

        public LineGraph() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            SnapsToDevicePixels = true;
            InitializeUserControl();
            InitializeOpenGL();
        }

        public void Cleanup() {
            foreach (var dl in _displayLists.Values) {
                dl.Dispose();
            }
            _leftTickBar.Dispose();
            _bottomTickBar.Dispose();
            _glcontrol.Dispose();
        }

        void InitializeUserControl() {
            _glcontrol = new GLControl();
            Child = _glcontrol;
            _glcontrol.Resize += (s, args) => SetWindowSize(_glcontrol.Width, _glcontrol.Height);
            _glcontrol.MouseDown += (s, args) => StartPan(args.X, args.Y);
            _glcontrol.MouseMove += (s, args) => Pan(args.X, args.Y);
            _glcontrol.MouseUp += (s, args) => StopPan();
            _glcontrol.MouseWheel += (s, args) => Zoom(args.Delta);
            _glcontrol.Paint += (s, args) => Draw();
            Loaded += (s, args) => Draw();
        }

        void InitializeOpenGL() {
            _glcontrol.MakeCurrent();

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.DontCare);

            _leftTickBar = new TickBar(TickBarOrientation.Vertical) {
                MinorTick = 1,
                MajorTick = 5
            };
            _bottomTickBar = new TickBar(TickBarOrientation.Horizontal) {
                MinorTick = 1,
                MajorTick = 5
            };
        }

        public void Draw() {
            if (Window == null) return;
            if (Window.WindowWidth == 0 || Window.WindowHeight == 0) return;
            _glcontrol.MakeCurrent();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);

            WindowMode();
            DrawHorizontalCrossBars();
            DrawVerticalCrossBars();

            DataMode();
            foreach (var dl in _displayLists.Values) {
                dl.Draw();
            }
            foreach (var m in _markers) m.Draw(Window);

            WindowMode();
            _bottomTickBar.Draw(Window);
            _leftTickBar.Draw(Window);

            _glcontrol.SwapBuffers();
        }

        public void StartPan(int xpos, int ypos) {
            _panningStarted = true;
            _xstart = xpos;
            _ystart = ypos;
        }

        public void StopPan() {
            _panningStarted = false;
        }

        public void Pan(int xpos, int ypos) {
            if (!_panningStarted) return;
            var xoffset = -(((xpos - _xstart) / (Window.WindowWidth * 1.0)) * (Window.Finish - Window.Start));
            var yoffset = ((ypos - _ystart) / (Window.WindowHeight * 1.0)) * (Window.Top - Window.Bottom);
            _xstart = xpos;
            _ystart = ypos;
            Window.Start += xoffset;
            Window.Finish += xoffset;
            Window.Bottom += yoffset;
            Window.Top += yoffset;
            Draw();
        }

        public void Zoom(double zdelta) {
            var percentageWidth = ((Window.Finish - Window.Start) / 100.0) * 10;
            var percentageHeight = ((Window.Top - Window.Bottom) / 100.0) * 10;

            if (zdelta > 0) {
                Window.Start += percentageWidth;
                Window.Finish -= percentageWidth;
                Window.Top -= percentageHeight;
                Window.Bottom += percentageHeight;
            } else {
                Window.Start -= percentageWidth;
                Window.Finish += percentageWidth;
                Window.Top += percentageHeight;
                Window.Bottom -= percentageHeight;
            }
            Draw();
        }

        public void SetWindowSize(int windowWidth, int windowHeight) {
            if (Window != null) {
                Window.WindowWidth = windowWidth;
                Window.WindowHeight = windowHeight;
            }
            _glcontrol.MakeCurrent();
            GL.Viewport(0, 0, windowWidth, windowHeight);
            GL.Flush();
            Draw();
        }

        void LoadDisplayLists() {
            foreach (var dl in _displayLists.Values) {
                dl.Dispose();
            }
            _displayLists.Clear();

            foreach (var line in _lines) {
                LoadDisplayList(line);
            }
        }

         void LoadDisplayList(Line line) {
            _displayLists[line] = new DisplayList(() => {
                GL.LineWidth(line.Thickness);
                GL.Begin(BeginMode.Lines);
                var size = line.Points.Length;
                GL.Color3(line.Color.R / 255.0, line.Color.G / 255.0,
                          line.Color.B / 255.0);
                for (var j = 0; j < size - 1; j++) {
                    var p1 = line.Points[j];
                    var p2 = line.Points[j + 1];
                    GL.Vertex2(p1.X / 1000.0, p1.Y);
                    GL.Vertex2(p2.X / 1000.0, p2.Y);
                }
                GL.End();
                GL.LineWidth(1.0f);
            });
        }

        void DrawHorizontalCrossBars() {
            if (Math.Abs(Window.WindowHeight) < 0.001 || Math.Abs(Window.WindowWidth) < 0.001) return;
            GL.Color3(0.878f, 0.878f, 0.878f);
            GL.Begin(BeginMode.Lines);
            var adjustedMajorTick = _leftTickBar.AdjustedMajorTick(Window);
            var start = TickBar.VerticalStart(Window, adjustedMajorTick);
            for (var i = start; i < Window.Top; i += adjustedMajorTick) {
                var r = new Point(0, i).ToScreen(Window);
                GL.Vertex2(50, r.Y);
                GL.Vertex2(Window.WindowWidth, r.Y);
            }
            GL.End();
        }

        void DrawVerticalCrossBars() {
            if (Math.Abs(Window.WindowHeight) < 0.001 || Math.Abs(Window.WindowWidth) < 0.001) return;
            GL.Color3(0.878f, 0.878f, 0.878f);
            GL.Begin(BeginMode.Lines);
            var adjustedMajorTick = _bottomTickBar.AdjustedMajorTick(Window);
            var start = TickBar.HorizontalStart(Window, adjustedMajorTick);
            for (var i = start; i < Window.Finish; i += adjustedMajorTick) {
                var r = new Point(i, 0).ToScreen(Window);
                GL.Vertex2(r.X, 50);
                GL.Vertex2(r.X, Window.WindowHeight);
            }
            GL.End();
        }

        void DataMode() {
            GL.LoadIdentity();
            GL.Ortho(Window.Start, Window.Finish, Window.Bottom, Window.Top, -1, 1);
        }

        void WindowMode() {
            GL.LoadIdentity();
            GL.Ortho(0, Window.WindowWidth, 0, Window.WindowHeight, -1, 1);
        }

        public void ReloadDisplay(IList<Line> lines, Rect rect) {
            if (Window == null) {
                Window = new GraphWindow();
            }
            Window.Start = rect.X - 15;
            Window.Finish = rect.X + rect.Width - 15;
            Window.Bottom = rect.Y;
            Window.Top = rect.Y + rect.Height;
            Window.WindowWidth = (int)ActualWidth;
            Window.WindowHeight = (int)ActualHeight;

            _glcontrol.MakeCurrent();

            _lines = new List<Line>(lines);
            LoadDisplayLists();
            Draw();
        }

        public void RemoveLine(Line oldLine){
            _lines.Remove(oldLine);
            var dl = _displayLists[oldLine];
            _displayLists.Remove(oldLine);
            dl.Dispose();
        }
        
        public void AddLine(Line newLine){
            _lines.Add(newLine);
            LoadDisplayList(newLine);
        }

        public void ClearMarkers() {
            _markers.Clear();
        }

        public void AddMarker(IDrawable marker) {
            _markers.Add(marker);
        }
    }

    public class Line {
        public Point[] Points { get; set; }
        public Color Color { get; set; }
        public float Thickness { get; set; }
        public Line(float thickness, Color color, Point[] points) {
            Color = color;
            Points = points;
            Thickness = thickness;
        }
    }

    public class GraphWindow {
        public int WindowHeight { get; set; }
        public int WindowWidth { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Start { get; set; }
        public double Finish { get; set; }
    }

    public interface ILineGraph {
        void Draw();
        void StartPan(int xpos, int ypos);
        void StopPan();
        void Pan(int xpos, int ypos);
        void Zoom(double zdelta);
        void SetWindowSize(int windowWidth, int windowHeight);
        void ReloadDisplay(IList<Line> lines, Rect rect);
        void ClearMarkers();
        void AddMarker(IDrawable marker);
        IDrawable[] Markers { get; }
        void Cleanup();
        void RemoveLine(Line oldLine);
        void AddLine(Line newLine);
    }
}
