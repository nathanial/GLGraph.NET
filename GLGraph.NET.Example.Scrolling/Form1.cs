using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using GLGraph.NET.Extensions;

namespace GLGraph.NET.Example.Scrolling {
    public partial class Form1 : Form {
        readonly LineGraph _graph;

        readonly DispatcherTimer _timer;

        public Form1() {
            InitializeComponent();

            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 30.0);
            _timer.Tick += delegate {
                _graph.Draw();
            };

            _graph = new LineGraph();
            _graph.TextEnabled = true;
            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowDynamicGraph();
                _timer.Start();
            };
        }

        void ShowDynamicGraph() {
            new Thread(() => {
                var line = new Line(1.0f, Color.Black.ToGLColor(), new GLPoint[] { }) {
                    IsDynamic = true
                };
                var i = 0;
                _graph.Display(new GLRect(i - 60, -20, 120, 50), false);
                BeginInvoke((Action)delegate {
                    _graph.Lines.Add(line);
                });
                while (true) {
                    var point = new GLPoint(i++, Math.Sin(i / 100.0) * 10 - 5);
                    BeginInvoke((Action)delegate {
                        if (line.Points.Count > 3000) {
                            line.RemovePoint(0, update: false);
                        }
                        line.AddPoint(point);
                        _graph.Display(new GLRect(i - 60, -20, 120, 50), false);
                    });
                    Thread.Sleep(10);
                }
            }).Start();
        }
    }
}
