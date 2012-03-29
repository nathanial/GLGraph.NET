using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace GLGraph.NET.Example {
    public partial class MainWindow {
        readonly LineGraph _graph;

        public MainWindow() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            _graph = new LineGraph();

            Loaded += delegate {
                host.Child = _graph.Control;
                ShowStaticGraph();
            };
        }

        void ShowStaticGraph() {
            _graph.Lines.Add(new Line(1.0f, Colors.Black.ToGLColor(), new[] {
                new GLPoint(0, 0),
                new GLPoint(1, 5),
                new GLPoint(2, 0),
                new GLPoint(3, 5),
                new GLPoint(4, 0),
                new GLPoint(5, 5), 
                new GLPoint(6, 0), 
                new GLPoint(7, 5), 
                new GLPoint(8, 0), 
                new GLPoint(9, 5), 
                new GLPoint(10, 0), 
            }));
            _graph.Display(new GLRect(0, 0, 10, 10), true);
        }

        DispatcherTimer _timer;

        void ShowDynamicGraph() {
            _graph.TextEnabled = true;

            var line1 = new Line(1.0f, Colors.Red.ToGLColor(), new GLPoint[] { });
            var line2 = new Line(1.0f, Colors.Green.ToGLColor(), new GLPoint[] { });
            var line3 = new Line(1.0f, Colors.Blue.ToGLColor(), new GLPoint[] { });
            _graph.Lines.Add(line1);
            _graph.Lines.Add(line2);
            _graph.Lines.Add(line3);
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.001) };

            var i = 0;
            var rect = new GLRect(0, 0, 500,100);
            var rand = new Random();
            _timer.Tick += delegate {
                line1.AddPoint(new GLPoint(i, (rand.Next() % 10) + 5));
                line2.AddPoint(new GLPoint(i, (rand.Next() % 10) + 15));
                line3.AddPoint(new GLPoint(i, (rand.Next() % 10) + 25));

                i++;

                if (i >= (rect.Width + rect.X)) {
                    line1.RemovePoint(0);
                    line2.RemovePoint(0);
                    line3.RemovePoint(0);

                    var d = (i - (rect.Width + rect.X));
                    rect = new GLRect(rect.X + d, rect.Y, rect.Width, rect.Height);
                    _graph.Display(rect, false);
                }
                _graph.Draw();
            };
            _timer.Start();
            _graph.Display(rect, true);
        }
    }

    public static class ColorExtensions {
        public static GLColor ToGLColor(this System.Drawing.Color color) {
            return new GLColor(color.A / 255.0, color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }

        public static GLColor ToGLColor(this System.Windows.Media.Color color) {
            return new GLColor(color.A / 255.0, color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
    }
}
