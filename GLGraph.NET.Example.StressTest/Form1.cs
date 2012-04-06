using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using GLGraph.NET.Extensions;
using Point = System.Drawing.Point;

namespace GLGraph.NET.Example.StressTest {
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
                    var origin =
                        _graph.Window.ScreenToView(new GLPoint(args.Location.X,
                                                               args.Location.Y));
                    var size = new GLSize(10, 1);

                    var group1 = new MenuItem("Group 1");
                    group1.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph,
                                                               origin, size,
                                                               Color.
                                                                   HotPink.
                                                                   ToGLColor
                                                                   ()));
                        _graph.Draw();
                    };

                    var group2 = new MenuItem("Group 2");
                    group2.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph,
                                                               origin, size,
                                                               Color.Blue.
                                                                   ToGLColor
                                                                   ()));
                        _graph.Draw();
                    };

                    var nox = new MenuItem("No Explosive");
                    nox.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin,
                                                               size,
                                                               Color.Aqua.
                                                                   ToGLColor()));
                        _graph.Draw();
                    };

                    var ofb = new MenuItem("Out Of Bounds");
                    ofb.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin,
                                                               size,
                                                               Color.Yellow.
                                                                   ToGLColor()));
                        _graph.Draw();
                    };

                    var dnt = new MenuItem("DNT");
                    dnt.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph, origin,
                                                               size,
                                                               Color.Orange.
                                                                   ToGLColor()));
                        _graph.Draw();
                    };

                    var explosive = new MenuItem("Explosive");
                    explosive.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph,
                                                               origin,
                                                               size,
                                                               Color.
                                                                   Maroon
                                                                   .
                                                                   ToGLColor
                                                                   ()));
                        _graph.Draw();
                    };

                    var peroxide = new MenuItem("Peroxide");
                    peroxide.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph,
                                                               origin,
                                                               size,
                                                               Color.
                                                                   Green.
                                                                   ToGLColor
                                                                   ()));
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

        DispatcherTimer _drawTimer;
        DispatcherTimer _clearTimer;
        DispatcherTimer _rectangleDrawer;
        DispatcherTimer _markerDrawer;

        void ShowStaticGraph() {
            var textures = new PersistentTextures();
            _graph.Display(new GLRect(0, -20, 1000, 50), true);
            var random = new Random();

            _clearTimer = new DispatcherTimer();
            _clearTimer.Interval = TimeSpan.FromMilliseconds(100);
            _clearTimer.Tick += delegate {
                var count = _graph.Lines.Count;
                if (count > 0) {
                    for (var i = 0; i < count / 2; i++) {
                        _graph.Lines.RemoveAt(0);
                    }
                }
                count = _graph.Markers.Count;
                if (count > 0) {
                    for (var i = 0; i < count / 2; i++) {
                        _graph.Markers.RemoveAt(0);
                    }
                }
                _graph.Draw();
            };
            _clearTimer.Start();

            _drawTimer = new DispatcherTimer();
            _drawTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _drawTimer.Tick += delegate {
                for (var i = 0; i < 500; i++) {
                    for (var j = 0; j < 3; j++) {
                        var data = new List<GLPoint>();
                        for (var k = 0; k < 100 * 2; k++) {
                            data.Add(new GLPoint(k / 2.0 * 10, random.NextDouble() * 30 - 15));
                        }
                        _graph.Lines.Add(new Line(1.0f, Color.Black.ToGLColor(), data.ToArray()));
                    }
                }
                _graph.Draw();
            };
            _drawTimer.Start();

            _rectangleDrawer = new DispatcherTimer();
            _rectangleDrawer.Interval = TimeSpan.FromMilliseconds(10);
            _rectangleDrawer.Tick += delegate {
                for (var i = 0; i < 10; i++) {
                    _graph.Markers.Add(new Rectangle(new GLColor(1, 0.5, 0.5, 0.5), true,
                                                     new GLPoint(random.NextDouble() * 1000,
                                                                 random.NextDouble() * 30 - 15),
                                                     new GLSize(random.NextDouble() * 10, 1)));
                }
            };
            _rectangleDrawer.Start();

            _markerDrawer = new DispatcherTimer();
            _markerDrawer.Interval = TimeSpan.FromMilliseconds(10);
            _markerDrawer.Tick += delegate {
                for (var i = 0; i < 10; i++) {
                    _graph.Markers.Add(
                        new SatisfiedMarker(
                            new GLPoint(random.NextDouble() * 1000, random.NextDouble() * 30 - 15),
                            textures.RedOrb));
                }
            };
            _markerDrawer.Start();

        }

        public class PersistentTextures : IDisposable {
            public readonly PersistentTexture BlueOrb = LoadTexture("Button-Blank-Blue-icon.png");
            public readonly PersistentTexture RedOrb = LoadTexture("Button-Blank-Red-icon.png");
            public readonly PersistentTexture YellowOrb = LoadTexture("Button-Blank-Yellow-icon.png");
            public readonly PersistentTexture GreenOrb = LoadTexture("Button-Blank-Green-icon.png");

            public void Dispose() {
                BlueOrb.Dispose();
                RedOrb.Dispose();
                YellowOrb.Dispose();
                GreenOrb.Dispose();
            }

            static PersistentTexture LoadTexture(string name) {
                using (var stream = File.OpenRead("Resources/Images/" + name)) {
                    var bitmap = new Bitmap(stream);
                    return new PersistentTexture(bitmap);
                }
            }
        }



        public class SatisfiedMarker : IDrawable {
            readonly GLPoint _pt;
            readonly PersistentTexture _texture;

            public SatisfiedMarker(GLPoint pt,
                                   PersistentTexture textures) {
                _pt = pt;
                _texture = textures;
                Visible = true;
            }

            public void Draw(GraphWindow window) {
                if (Visible) {
                    _texture.Draw(window, new Point((int)_pt.X, (int)_pt.Y));
                }
            }

            public bool Visible { get; set; }

            public Rect Dimensions {
                get { return Rect.Empty; }
            }
        }
    }


}
