using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using SharpGL;

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
        
        public Line(float thickness, Color color, IEnumerable<Point> points) {
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
        public double WindowHeight;
        public double WindowWidth;

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

        double LeftMargin { get; }
        double BottomMargin { get; }
        double XOffset { get; }
        double YOffset { get; }

        bool TextEnabled { get; set; }
    }

    public partial class LineGraph : ILineGraph {
        readonly ObservableCollection<IDrawable> _markers = new ObservableCollection<IDrawable>();
        readonly ObservableCollection<Line> _lines = new ObservableCollection<Line>();
        readonly IDictionary<Line, DisplayList> _displayLists = new Dictionary<Line, DisplayList>();

        bool _panningStarted;
        TickBar _leftTickBar;
        TickBar _bottomTickBar;
        double _xstart, _ystart;

        GraphWindow Window { get; set; }

        public ObservableCollection<IDrawable> Markers { get { return _markers; } }
        public ObservableCollection<Line> Lines { get { return _lines; } }

        public bool TextEnabled { get; set; }

        const string ZeroError = "WindowWidth cannot be zero, consider initializing LineGraph in the host's Loaded event";

        OpenGL _gl;

        public LineGraph() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            TextEnabled = true;
            SnapsToDevicePixels = true;


            _lines.CollectionChanged += (s, args) => {
                if (args.Action == NotifyCollectionChangedAction.Reset) {
                    foreach (var d in _displayLists.Values) {
                        d.Dispose(_gl);
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
                dl.Dispose(_gl);
            }
            _leftTickBar.Dispose(_gl);
            _bottomTickBar.Dispose(_gl);
        }

        public void Draw() {
            if (Window == null) return;
            if (Window.WindowWidth == 0 || Window.WindowHeight == 0) return;
            _gl.MakeCurrent();

            _gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            _gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            _gl.MatrixMode(OpenGL.GL_PROJECTION);

            WindowMode();
            DrawHorizontalCrossBars();
            DrawVerticalCrossBars();

            DataMode();
            foreach (var dl in _displayLists.Values) {
                dl.Draw(_gl);
            }
            foreach (var m in _markers) m.Draw(Window);

            WindowMode();
            _bottomTickBar.Draw(_gl,Window);
            _leftTickBar.Draw(_gl,Window);

            _gl.Flush();
        }

        public void StartPan(double xpos, double ypos) {
            _panningStarted = true;
            _xstart = xpos;
            _ystart = ypos;
        }

        public void StopPan() {
            _panningStarted = false;
        }

        public void Pan(double xpos, double ypos) {
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


            _gl.MakeCurrent();

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
            dl.Dispose(_gl);
            _displayLists.Remove(line);
            line.Changed -= LineChanged;
        }

        void InitializeUserControl() {
            SizeChanged += (s, args) => SetWindowSize(ActualWidth, ActualHeight);
            MouseDown += (s, args) => StartPan(args.GetPosition(this).X, args.GetPosition(this).Y);
            MouseMove += (s, args) => Pan(args.GetPosition(this).X, args.GetPosition(this).Y);
            MouseUp += (s, args) => StopPan();
            MouseWheel += (s, args) => Zoom(args.Delta);
            //GraphWindow += (s, args) => Draw();
            Loaded += (s, args) => Draw();
            
        }


        void InitializeOpenGL() {
            _gl.MakeCurrent();
            _gl.Enable(OpenGL.GL_TEXTURE_2D);
            _gl.Enable(OpenGL.GL_LINE_SMOOTH);
            _gl.Enable(OpenGL.GL_BLEND);
            _gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA); //maybe wrong
            _gl.Hint(OpenGL.GL_LINE_SMOOTH, OpenGL.GL_DONT_CARE);

            _leftTickBar = new TickBar(this,TickBarOrientation.Vertical) {
                MinorTick = 1,
                MajorTick = 5
            };
            _bottomTickBar = new TickBar(this, TickBarOrientation.Horizontal) {
                MinorTick = 1,
                MajorTick = 5
            };
        }

        void SetWindowSize(double windowWidth, double windowHeight) {
            if (Window != null) {
                Window.WindowWidth = windowWidth;
                Window.WindowHeight = windowHeight;
            }
            _gl.MakeCurrent();
            _gl.Viewport(0, 0, (int)windowWidth, (int)windowHeight);
            _gl.Flush();
            Draw();
        }

        void LoadDisplayLists() {
            foreach (var dl in _displayLists.Values) {
                dl.Dispose(_gl);
            }
            _displayLists.Clear();

            foreach (var line in _lines) {
                LoadDisplayList(line);
            }
        }

        void LoadDisplayList(Line line) {
            _displayLists[line] = new DisplayList(_gl, () => {
                _gl.LineWidth(line.Thickness);
                _gl.Begin(OpenGL.GL_LINES);
                var size = line.Points.Count;
                _gl.Color(line.Color.R / 255.0, line.Color.G / 255.0,
                          line.Color.B / 255.0, line.Color.A / 255.0);
                for (var j = 0; j < size - 1; j++) {
                    var p1 = line.Points[j];
                    var p2 = line.Points[j + 1];
                    _gl.Vertex(p1.X, p1.Y);
                    _gl.Vertex(p2.X, p2.Y);
                }
                _gl.End();
                _gl.LineWidth(1.0f);
            });
        }


        void DrawHorizontalCrossBars() {
            if (Math.Abs(Window.WindowHeight) < 0.001 || Math.Abs(Window.WindowWidth) < 0.001) return;
            _gl.Color(0.878f, 0.878f, 0.878f);
            _gl.Begin(OpenGL.GL_LINES);
            var adjustedMajorTick = _leftTickBar.AdjustedMajorTick(Window);
            var start = TickBar.VerticalStart(Window, adjustedMajorTick);
            for (var i = start; i < Window.Top; i += adjustedMajorTick) {
                var r = new Point(0, i + YOffset).ToScreen(Window);
                _gl.Vertex(LeftMargin, r.Y);
                _gl.Vertex(Window.WindowWidth, r.Y);
            }
            _gl.End();
        }

        void DrawVerticalCrossBars() {
            if (Math.Abs(Window.WindowHeight) < 0.001 || Math.Abs(Window.WindowWidth) < 0.001) return;
            _gl.Color(0.878f, 0.878f, 0.878f);
            _gl.Begin(OpenGL.GL_LINES);
            var adjustedMajorTick = _bottomTickBar.AdjustedMajorTick(Window);
            var start = TickBar.HorizontalStart(Window, adjustedMajorTick);
            for (var i = start; i < Window.Finish; i += adjustedMajorTick) {
                var r = new Point(i + XOffset, 0).ToScreen(Window);
                _gl.Vertex(r.X, BottomMargin);
                _gl.Vertex(r.X, Window.WindowHeight);
            }
            _gl.End();
        }

        void DataMode() {
            _gl.LoadIdentity();
            var xoffset = XOffset;
            var yoffset = YOffset;
            _gl.Ortho(Window.Start - xoffset, Window.Finish - xoffset, Window.Bottom - yoffset , Window.Top - yoffset, -1, 1);
        }

        void WindowMode() {
            _gl.LoadIdentity();
            _gl.Ortho(0, Window.WindowWidth, 0, Window.WindowHeight, -1, 1);
        }


        void LineChanged(object sender, LineChangedEventArgs e) {
            var dl = _displayLists[e.Line];
            dl.Dispose(_gl);
            _displayLists.Remove(e.Line);
            LoadDisplayList(e.Line);
        }

        public double LeftMargin { get { return 50; } }
        public double BottomMargin { get { return 50; } }

        public double XOffset {
            get { return new Point(LeftMargin, 0).ToView(Window).X; }
        }

        public double YOffset {
            get { return new Point(0, BottomMargin).ToView(Window).Y; }
        }

        void OpenGLControlOpenGLDraw(object sender, OpenGLEventArgs args) {
            Draw();
        }

        void OpenGLControlOpenGLInitialized(object sender, OpenGLEventArgs args) {
            _gl = args.OpenGL;
            InitializeUserControl();
            InitializeOpenGL();
        }
    }

}
