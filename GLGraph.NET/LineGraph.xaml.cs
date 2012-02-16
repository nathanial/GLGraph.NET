using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {

    public class LineChangedEventArgs : EventArgs {
        public Line Line { get; set; }
        public LineChangedEventArgs(Line line) {
            Line = line;
        }
    }

    public class Line {
        public event EventHandler<LineChangedEventArgs> Changed;

        public IList<Point> Points { get; set; }
        public Color Color { get; set; }
        public float Thickness { get; set; }
        
        public Line(float thickness, Color color, IList<Point> points) {
            var copy = new List<Point>();
            copy.AddRange(points);

            Color = color;
            Thickness = thickness;
            Points = copy;
        }

        public void AddPoint(Point point) {
            Points.Add(point);
            if(Changed != null) {
                Changed(this, new LineChangedEventArgs(this));
            }
        }

        public void RemovePoint(int index) {
            Points.RemoveAt(index);
            if(Changed != null) {
                Changed(this, new LineChangedEventArgs(this));
            }
        }


    }

    public class GraphWindow {
        public Point DataOrigin;
        public double DataWidth;
        public double DataHeight;
        public int WindowHeight;
        public int WindowWidth;

        public double Top {
            get { return DataOrigin.Y + DataHeight; }
        }

        public double Finish {
            get { return DataOrigin.X + DataWidth;  }
        }

        public double Start {
            get { return DataOrigin.X; }
        }

        public double Bottom {
            get { return DataOrigin.Y; }
        }
    }

    public interface ILineGraph {
        ObservableCollection<Line> Lines { get; }
        ObservableCollection<IDrawable> Markers { get; }

        void Draw();
        void Display(Rect rect, bool draw);
        void Cleanup();
    }

    public partial class LineGraph : ILineGraph {
        readonly ObservableCollection<IDrawable> _markers = new ObservableCollection<IDrawable>();
        readonly ObservableCollection<Line> _lines = new ObservableCollection<Line>();
        readonly IDictionary<Line, DisplayList> _displayLists = new Dictionary<Line, DisplayList>();

        bool _panningStarted;
        TickBar _leftTickBar;
        TickBar _bottomTickBar;
        int _xstart, _ystart;

        GraphWindow Window { get; set; }

        public ObservableCollection<IDrawable> Markers { get { return _markers; } }
        public ObservableCollection<Line> Lines { get { return _lines; } }

        GLControl _glcontrol;
        const string ZeroError = "WindowWidth cannot be zero, consider initializing LineGraph in the host's Loaded event";

        public LineGraph() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            SnapsToDevicePixels = true;
            InitializeUserControl();
            InitializeOpenGL();

            _lines.CollectionChanged += (s, args) => {
                if (args.Action == NotifyCollectionChangedAction.Reset) {
                    foreach (var d in _displayLists.Values) {
                        d.Dispose();
                    }
                    _displayLists.Clear();
                }
                if (args.OldItems != null) {
                    foreach (Line line in args.OldItems) {
                        RemoveLine(line);
                    }
                }
                if (args.NewItems != null) {
                    foreach (Line line in args.NewItems) {
                        AddLine(line);
                    }
                }
            };
        }

        public void Cleanup() {
            foreach (var dl in _displayLists.Values) {
                dl.Dispose();
            }
            _leftTickBar.Dispose();
            _bottomTickBar.Dispose();
            _glcontrol.Dispose();
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
            var xoffset = -(((xpos - _xstart) / (Window.WindowWidth * 1.0)) * (Window.DataWidth));
            var yoffset = ((ypos - _ystart) / (Window.WindowHeight * 1.0)) * (Window.DataHeight);
            _xstart = xpos;
            _ystart = ypos;
            Window.DataOrigin = new Point(Window.DataOrigin.X + xoffset, Window.DataOrigin.Y + yoffset);
            Draw();
        }

        public void Zoom(double zdelta) {
            var percentageWidth = ((Window.DataWidth) / 100.0) * 10;
            var percentageHeight = ((Window.DataHeight) / 100.0) * 10;

            if (zdelta > 0) {
                Window.DataWidth -= percentageWidth;
                Window.DataHeight -= percentageHeight;
                Window.DataOrigin = new Point(Window.DataOrigin.X + percentageWidth / 2.0, 
                                              Window.DataOrigin.Y + percentageHeight / 2.0);
            } else {
                Window.DataWidth += percentageWidth;
                Window.DataHeight += percentageHeight;
                Window.DataOrigin = new Point(Window.DataOrigin.X - percentageWidth / 2.0,
                                              Window.DataOrigin.Y - percentageHeight /2.0);
            }
            Draw();
        }

        public void Display(Rect rect, bool draw) {
            if (Window == null) {
                Window = new GraphWindow();
            }
            Window.DataOrigin = rect.Location;
            Window.DataWidth = rect.Width;
            Window.DataHeight = rect.Height;
            Window.WindowWidth = (int)ActualWidth;
            Window.WindowHeight = (int)ActualHeight;

            if (Window.WindowWidth == 0) throw new Exception(ZeroError);
            if (Window.WindowHeight == 0) throw new Exception(ZeroError);


            _glcontrol.MakeCurrent();

            LoadDisplayLists();
            if (draw) {
                Draw();
            }
        }

        void AddLine(Line line) {
            LoadDisplayList(line);
            line.Changed += LineChanged;
        }

        void RemoveLine(Line line) {
            var dl = _displayLists[line];
            dl.Dispose();
            _displayLists.Remove(line);
            line.Changed -= LineChanged;
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


        float _lineMin;
        float _lineMax;
        float _lineGranularity;
        //GL_SMOOTH_LINE_WIDTH_GRANULARITY

        void InitializeOpenGL() {
            _glcontrol.MakeCurrent();
            var range = new float[2];
            GL.GetFloat(GetPName.SmoothLineWidthRange, range);
            _lineMin = range[0];
            _lineMax = range[1];
            GL.GetFloat(GetPName.SmoothLineWidthGranularity, out _lineGranularity);

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

        void SetWindowSize(int windowWidth, int windowHeight) {
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
                GL.LineWidth(ConstrainThickness(line.Thickness));
                GL.Begin(BeginMode.Lines);
                var size = line.Points.Count;
                GL.Color4(line.Color.R / 255.0, line.Color.G / 255.0,
                          line.Color.B / 255.0, line.Color.A / 255.0);
                for (var j = 0; j < size - 1; j++) {
                    var p1 = line.Points[j];
                    var p2 = line.Points[j + 1];
                    GL.Vertex2(p1.X, p1.Y);
                    GL.Vertex2(p2.X, p2.Y);
                }
                GL.End();
                GL.LineWidth(_lineMin);
            });
        }


        void DrawHorizontalCrossBars() {
            if (Math.Abs(Window.WindowHeight) < 0.001 || Math.Abs(Window.WindowWidth) < 0.001) return;
            GL.Color3(0.878f, 0.878f, 0.878f);
            GL.Begin(BeginMode.Lines);
            var adjustedMajorTick = _leftTickBar.AdjustedMajorTick(Window);
            var start = TickBar.VerticalStart(Window, adjustedMajorTick);
            for (var i = start; i < Window.Top; i += adjustedMajorTick) {
                var r = new Point(0, i + new Point(0,50).ToView(Window).Y).ToScreen(Window);
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
                var r = new Point(i + new Point(50,0).ToView(Window).X, 0).ToScreen(Window);
                GL.Vertex2(r.X, 50);
                GL.Vertex2(r.X, Window.WindowHeight);
            }
            GL.End();
        }

        void DataMode() {
            GL.LoadIdentity();
            var xoffset = new Point(50, 0).ToView(Window).X;
            var yoffset = new Point(0, 50).ToView(Window).Y;
            GL.Ortho(Window.Start - xoffset, Window.Finish - xoffset, Window.Bottom - yoffset , Window.Top - yoffset, -1, 1);
        }

        void WindowMode() {
            GL.LoadIdentity();
            GL.Ortho(0, Window.WindowWidth, 0, Window.WindowHeight, -1, 1);
        }


        float ConstrainThickness(float thickness) {
            var t = Math.Min(Math.Max(thickness, _lineMin), _lineMax);
            var step = ((int)(t * 10)) / ((int)(_lineGranularity * 10));
            var r = step * _lineGranularity;
            return r;
        }


        void LineChanged(object sender, LineChangedEventArgs e) {
            var dl = _displayLists[e.Line];
            dl.Dispose();
            _displayLists.Remove(e.Line);
            LoadDisplayList(e.Line);
        }


    }


}
