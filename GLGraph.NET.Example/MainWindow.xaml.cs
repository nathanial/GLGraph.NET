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
            _graph.Lines.Add(new Line(1.0f, Colors.Black, new[] {
                new Point(0, 0),
                new Point(1, 5),
                new Point(2, 0),
                new Point(3, 5),
                new Point(4, 0),
                new Point(5, 5), 
                new Point(6, 0), 
                new Point(7, 5), 
                new Point(8, 0), 
                new Point(9, 5), 
                new Point(10, 0), 
            }));
            _graph.Display(new Rect(0, 0, 10, 10), true);
        }

        DispatcherTimer _timer;

        void ShowDynamicGraph() {
            _graph.TextEnabled = true;

            var line1 = new Line(1.0f, Colors.Red, new Point[] { });
            var line2 = new Line(1.0f, Colors.Green, new Point[] { });
            var line3 = new Line(1.0f, Colors.Blue, new Point[] { });
            _graph.Lines.Add(line1);
            _graph.Lines.Add(line2);
            _graph.Lines.Add(line3);
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.001) };

            var i = 0;
            var rect = new Rect(0, 0, 500,100);
            var rand = new Random();
            _timer.Tick += delegate {
                line1.AddPoint(new Point(i, (rand.Next() % 10) + 5));
                line2.AddPoint(new Point(i, (rand.Next() % 10) + 15));
                line3.AddPoint(new Point(i, (rand.Next() % 10) + 25));

                i++;

                if (i >= (rect.Width + rect.X)) {
                    line1.RemovePoint(0);
                    line2.RemovePoint(0);
                    line3.RemovePoint(0);

                    var d = (i - (rect.Width + rect.X));
                    rect = new Rect(rect.X + d, rect.Y, rect.Width, rect.Height);
                    _graph.Display(rect, false);
                }
                _graph.Draw();
            };
            _timer.Start();
            _graph.Display(rect, true);
        }
    }
}
