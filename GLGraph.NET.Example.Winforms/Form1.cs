using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GLGraph.NET.Extensions;
using Point = System.Drawing.Point;

namespace GLGraph.NET.Example.Winforms {
    public partial class Form1 : Form {
        readonly LineGraph _graph;



        public Form1() {
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;



            _graph = new LineGraph();

            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowStaticGraph();
            };

            _graph.Control.MouseClick += (s, args) => {
                if (args.Button == MouseButtons.Right) {
                    var origin = _graph.Window.ScreenToView(new GLPoint(args.Location.X, args.Location.Y));
                    var size = new GLSize(10, 1);

                    var group1 = new MenuItem("Group 1");
                    group1.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.HotPink.ToGLColor()));
                        _graph.Draw();
                    };

                    var group2 = new MenuItem("Group 2");
                    group2.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Blue.ToGLColor()));
                        _graph.Draw();
                    };

                    var nox = new MenuItem("No Explosive");
                    nox.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Aqua.ToGLColor()));
                        _graph.Draw();
                    };

                    var ofb = new MenuItem("Out Of Bounds");
                    ofb.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Yellow.ToGLColor()));
                        _graph.Draw();
                    };

                    var dnt = new MenuItem("DNT");
                    dnt.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Orange.ToGLColor()));
                        _graph.Draw();
                    };

                    var explosive = new MenuItem("Explosive");
                    explosive.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Maroon.ToGLColor()));
                        _graph.Draw();
                    };

                    var peroxide = new MenuItem("Peroxide");
                    peroxide.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin, size, Color.Green.ToGLColor()));
                        _graph.Draw();
                    };

                    var menu = new ContextMenu {
                        MenuItems = {
                            group1,
                            group2,
                            nox,
                            ofb,
                            dnt,
                            explosive,
                            peroxide
                        }
                    };
                    menu.Show(_graph.Control, args.Location);
                }
            };
        }

        void ShowStaticGraph() {
            var data = new List<GLPoint>();
            var random = new Random();
            for (var i = 0; i < 100; i++) {
                data.Add(new GLPoint(i, random.NextDouble() * 30 - 15));
            }
            _graph.Lines.Add(new Line(1.0f, Color.Black.ToGLColor(), data.ToArray()));
            _graph.Display(new GLRect(0, -20, 120, 50), true);
        }

    }


}
