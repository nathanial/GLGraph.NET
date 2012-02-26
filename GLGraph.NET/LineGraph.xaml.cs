﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

        bool TextEnabled { get; set; }
    }

    public partial class LineGraph : ILineGraph {
        readonly ObservableCollection<IDrawable> _markers = new ObservableCollection<IDrawable>();
        readonly ObservableCollection<Line> _lines = new ObservableCollection<Line>();
        readonly IDictionary<Line, DisplayList> _displayLists = new Dictionary<Line, DisplayList>();

        bool _panningStarted;
        ITickBar _leftTickBar;
        ITickBar _bottomTickBar;
        int _xstart, _ystart;

        GraphWindow Window { get; set; }

        public ObservableCollection<IDrawable> Markers { get { return _markers; } }
        public ObservableCollection<Line> Lines { get { return _lines; } }

        public bool TextEnabled { get; set; }

        GLControl _glcontrol;
        const string ZeroError = "WindowWidth cannot be zero, consider initializing LineGraph in the host's Loaded event";

        public LineGraph() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            TextEnabled = true;
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
            GL.LoadIdentity();

            DrawData();
            DrawMarkers();
            DrawTicks();

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

        void DrawDeadSpace() {
            GL.Color3(System.Drawing.Color.White);
            OpenGL.DrawQuad(new Point(0,50),new Point(50,50),
                            new Point(50,0), new Point(0,0));
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

            _leftTickBar = new VerticalTickBar{
                MinorTick = 1,
                MajorTick = 5
            };
            _bottomTickBar = new HorizontalTickBar {
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


        void DataMode() {
            GL.LoadIdentity();
            GL.Ortho(Window.Start, Window.Finish, Window.Bottom, Window.Top, -1, 1);
            //var xo = new Point(50, 0).ToView(Window).X;
            //var yo = new Point(0,50).ToView(Window).Y;
            var xo = 0;
            var yo = 0;
            GL.Translate(xo,yo,0);
            GL.Scale((Window.DataWidth - xo) / Window.DataWidth, (Window.DataHeight - yo) / Window.DataHeight, 0);
        }

        void TickMode() {
            GL.LoadIdentity();
            GL.Ortho(0,100,Window.Bottom,Window.Top, -1,1);
            //var xo = new Point(0, 0).ToView(Window).X;
            //var yo = new Point(0, 50).ToView(Window).Y;
            var xo = 1.5;
            var yo = 0;
            GL.Translate(xo, yo, 0);
            GL.Scale(3, (Window.DataHeight - yo) / Window.DataHeight, 0);
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

        void DrawTicks() {
            TickMode();
            _leftTickBar.Window = Window;
            _bottomTickBar.Window = Window;
            _leftTickBar.MajorTick = 5;
            _leftTickBar.MinorTick = 1;
            _leftTickBar.TickStart = 0;

            _leftTickBar.RangeStart = Math.Floor(Window.Bottom);
            _leftTickBar.RangeStop = Math.Ceiling(Window.Top);

            Debug.WriteLine(_leftTickBar.RangeStart + " " + _leftTickBar.RangeStop);

            
            _bottomTickBar.Draw();
            _leftTickBar.Draw();
        }

        void DrawMarkers() {
            foreach (var m in _markers) m.Draw(Window);
        }

        void DrawData() {
            DataMode();
            foreach (var dl in _displayLists.Values) {
                dl.Draw();
            }
        }


    }

}
