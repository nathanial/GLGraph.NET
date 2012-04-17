using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;

namespace GLGraph.NET.Example.ColorChange {
    public partial class Form1 : Form {
        readonly LineGraph _graph;
        readonly Random _random = new Random();

        DispatcherTimer _timer;


        public Form1() {
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _graph = new LineGraph();
            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowStaticGraph();
            };
        }

        void ShowStaticGraph() {
            var line = new Line(1.0f, new GLColor(1.0, 0.0, 1.0, 0.0), new[] {
                new GLPoint(0,0),                                                                              
                new GLPoint(1,1), 
                new GLPoint(2,2), 
                new GLPoint(3,3), 
                new GLPoint(4,4), 
                new GLPoint(5,4),
                new GLPoint(6,3),
                new GLPoint(7,2), 
                new GLPoint(8,1), 
            });
            _graph.Lines.Add(line);
            _graph.Display(new GLRect(0, 0, 10, 10), true);

            _timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            _timer.Tick += delegate {
                var c = line.Color;
                line.Color = new GLColor(c.A, Adjust(c.R), Adjust(c.G), Adjust(c.B));
                _graph.Draw();
            };
            _timer.Start();

        }

        double Adjust(double d) {
            return Bound(d + Rand());
        }

        double Bound(double d) {
            return Math.Max(Math.Min(1.0, d),0);
        }

        double Rand() {
            return _random.NextDouble() - 0.5;
        }


    }
}
