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

        bool _dragging;
        HitKind _theHit;
        ThresholdMarker _theDragged;
        Point? _dragStart;


        public Form1() {
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            var hand = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_hand.png"), 8, 8);
            var handDrag = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_drag_hand.png"), 8, 8);

            _graph = new LineGraph();

            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowStaticGraph();
            };

            _graph.Control.MouseMove += (s, args) => {
                if (_dragging) {
                    if (_theDragged == null) return;
                    if (_dragStart == null) return;
                    switch (_theHit) {

                        case HitKind.None:
                        case HitKind.BottomEdge:
                        case HitKind.TopEdge:
                            break;

                        case HitKind.LeftEdge:
                            _theDragged.ResizeLeft(_graph.Window, _dragStart.Value, args.Location);
                            _dragStart = args.Location;
                            _graph.Draw();
                            break;
                        case HitKind.RightEdge:
                            _theDragged.ResizeRight(_graph.Window, _dragStart.Value, args.Location);
                            _dragStart = args.Location;
                            _graph.Draw();
                            break;

                        case HitKind.Center:
                            _theDragged.Drag(_graph.Window, _dragStart.Value, args.Location);
                            _dragStart = new Point(args.Location.X, args.Location.Y);
                            _graph.Draw();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } else {
                    if (_dragging) return;
                    var thresholds = _graph.Markers.OfType<ThresholdMarker>().ToArray();
                    if (thresholds.Length == 0) return;

                    var hit = thresholds.Select(x => new {
                        Threshold = x,
                        Hit = x.HitTest(_graph.Window, args.Location)
                    }).FirstOrDefault(x => x.Hit != HitKind.None);

                    if (hit == null) {
                        _graph.PanningIsEnabled = true;
                        Cursor = Cursors.Default;
                        _theDragged = null;
                        return;
                    }
                    switch (hit.Hit) {
                        case HitKind.LeftEdge:
                        case HitKind.RightEdge:
                            Cursor = Cursors.SizeWE;
                            _theDragged = hit.Threshold;
                            _theHit = hit.Hit;
                            break;

                        case HitKind.TopEdge:
                        case HitKind.BottomEdge:
                            //do nothing
                            break;

                        case HitKind.Center:
                            Cursor = hand;
                            _theDragged = hit.Threshold;
                            _theHit = hit.Hit;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            };

            _graph.Control.MouseDown += (s, args) => {
                if (args.Button == MouseButtons.Left) {
                    if (_theDragged == null) return;
                    switch (_theHit) {
                        case HitKind.LeftEdge:
                        case HitKind.RightEdge:
                            _graph.PanningIsEnabled = false;
                            _dragging = true;
                            _dragStart = args.Location;
                            break;

                        case HitKind.None:
                        case HitKind.TopEdge:
                        case HitKind.BottomEdge:
                            //do nothing
                            break;
                        case HitKind.Center:
                            _graph.PanningIsEnabled = false;
                            Cursor = handDrag;
                            _dragging = true;
                            _dragStart = args.Location;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            };

            _graph.Control.MouseUp += (s, args) => {
                _graph.PanningIsEnabled = true;
                _dragging = false;
                switch (_theHit) {
                    case HitKind.None:
                        break;
                    case HitKind.LeftEdge:
                        break;
                    case HitKind.TopEdge:
                        break;
                    case HitKind.BottomEdge:
                        break;
                    case HitKind.RightEdge:
                        break;
                    case HitKind.Center:
                        Cursor = hand;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };


            _graph.Control.MouseClick += (s, args) => {
                if (args.Button == MouseButtons.Right) {
                    var origin = _graph.Window.ScreenToView(new GLPoint(args.Location.X, args.Location.Y));
                    var size = new GLSize(10, 1);

                    var group1 = new MenuItem("Group 1");
                    group1.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.HotPink.ToGLColor()));
                        _graph.Draw();
                    };

                    var group2 = new MenuItem("Group 2");
                    group2.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Blue.ToGLColor()));
                        _graph.Draw();
                    };

                    var nox = new MenuItem("No Explosive");
                    nox.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Aqua.ToGLColor()));
                        _graph.Draw();
                    };

                    var ofb = new MenuItem("Out Of Bounds");
                    ofb.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Yellow.ToGLColor()));
                        _graph.Draw();
                    };

                    var dnt = new MenuItem("DNT");
                    dnt.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Orange.ToGLColor()));
                        _graph.Draw();
                    };

                    var explosive = new MenuItem("Explosive");
                    explosive.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Maroon.ToGLColor()));
                        _graph.Draw();
                    };

                    var peroxide = new MenuItem("Peroxide");
                    peroxide.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(origin, size, Color.Green.ToGLColor()));
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
