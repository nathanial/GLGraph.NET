using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace GLGraph.NET.Example.Winforms {
    public partial class Form1 : Form {
        readonly LineGraph _graph;

        bool _dragging;
        ThresholdMarker _theDragged;
        Point? _dragStart;

        public Form1() {
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            var hand = CustomCursor.CreateCursor((Bitmap) Image.FromFile("Cursors\\cursor_hand.png"), 8, 8);
            var handDrag = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_drag_hand.png"), 8, 8);

            _graph = new LineGraph();

            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowStaticGraph();
            };

            _graph.Control.MouseMove += (s, args) => {
                if (_dragging) {
                    _theDragged.Drag(_graph.Window, _dragStart.Value, args.Location);
                    _dragStart = new Point(args.Location.X, args.Location.Y);
                    _graph.Draw();
                } else {
                    var thresholds = _graph.Markers.OfType<ThresholdMarker>().ToArray();
                    if (thresholds.Length == 0) return;
                    var wloc = new Point(args.Location.X, args.Location.Y);
                    var hit = thresholds.FirstOrDefault(x => x.ScreenPosition(_graph.Window).Contains(wloc));
                    
                    if (hit != null) {
                        _theDragged = hit;
                        if (Cursor != hand && Cursor != handDrag) {
                            Cursor = hand;
                        }
                    } else {
                        _graph.PanningIsEnabled = true;
                        Cursor = System.Windows.Forms.Cursors.Default;
                    }
                }
            };

            _graph.Control.MouseDown += (s, args) => {
                if (args.Button == MouseButtons.Left) {
                    if (Cursor == hand) {
                        _graph.PanningIsEnabled = false;
                        Cursor = handDrag;
                        _dragging = true;
                        _dragStart = new Point(args.Location.X, args.Location.Y);
                    }
                }
            };

            _graph.Control.MouseUp += (s, args) => {
                if(Cursor == handDrag) {
                    _graph.PanningIsEnabled = true;
                    Cursor = hand;
                    _dragging = false;
                    _theDragged = null;
                    _dragStart = null;
                }
            };


            _graph.Control.MouseClick += (s, args) => {
                if (args.Button == MouseButtons.Right) {
                    var wloc = new Point(args.Location.X, args.Location.Y);
                    var newThreshold = new MenuItem("New Threshold");
                    newThreshold.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph.Window, wloc, new Size(10, 1)));
                        _graph.Draw();
                    };
                    var menu = new ContextMenu {
                        MenuItems = {
                            newThreshold
                        }
                    };
                    menu.Show(_graph.Control, args.Location);
                }
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

        public const uint LVM_SETHOTCURSOR = 4158;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


    }


    public class ThresholdMarker : IDrawable {
        readonly GLRectangle _rectangle;
        public ThresholdMarker(GraphWindow window, Point location, Size size) {
            _rectangle = new GLRectangle(new GLColor(1.0, 0.0, 0.0, 1.0), true, location, size);
            _rectangle.Origin = window.ScreenToView(location);
            _rectangle.Origin = new Point(_rectangle.Origin.X - size.Width / 2.0, _rectangle.Origin.Y - size.Height / 2.0);
        }

        public void Draw(GraphWindow window) {
            _rectangle.Draw();
        }

        public Rect ScreenPosition(GraphWindow window) {
            var origin = window.ViewToScreen(_rectangle.Origin);
            var corner = window.ViewToScreen(new Point(_rectangle.Origin.X + _rectangle.Size.Width, _rectangle.Origin.Y + _rectangle.Size.Height));
            return new Rect(origin, corner);
        }

        public void Drag(GraphWindow window, Point start, System.Drawing.Point location) {
            var locD = window.ScreenToView(new Point(location.X, location.Y));
            var startD = window.ScreenToView(new Point(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            var offsetY = locD.Y - startD.Y;
            _rectangle.Origin = new Point(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y + offsetY);
        }
    }
}
