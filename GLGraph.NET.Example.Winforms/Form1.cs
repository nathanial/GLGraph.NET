using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

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
                    var wloc = new GLPoint(args.Location.X, args.Location.Y);
                    var hit = thresholds.FirstOrDefault(x => x.ScreenPosition(_graph.Window).Contains(wloc.X, wloc.Y));
                    
                    if (hit != null) {
                        _theDragged = hit;
                        if (Cursor != hand && Cursor != handDrag) {
                            Cursor = hand;
                        }
                    } else {
                        _graph.PanningIsEnabled = true;
                        Cursor = Cursors.Default;
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
                    var wloc = new GLPoint(args.Location.X, args.Location.Y);
                    var newThreshold = new MenuItem("New Threshold");
                    newThreshold.Click += delegate {
                        _graph.Markers.Add(new ThresholdMarker(_graph.Window, wloc, new GLSize(10, 1)));
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
            _graph.Lines.Add(new Line(1.0f, Color.Black.ToGLColor(), new[] {
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

    }


    public class ThresholdMarker : IDrawable {
        readonly GLRectangle _rectangle;
        public ThresholdMarker(GraphWindow window, GLPoint location, GLSize size) {
            _rectangle = new GLRectangle(new GLColor(1.0, 0.0, 0.0, 1.0), true, location, size);
            _rectangle.Origin = window.ScreenToView(location);
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X - size.Width / 2.0, _rectangle.Origin.Y - size.Height / 2.0);
        }

        public void Draw(GraphWindow window) {
            _rectangle.Draw();
        }

        public GLRect ScreenPosition(GraphWindow window) {
            var origin = window.ViewToScreen(_rectangle.Origin);
            var corner = window.ViewToScreen(new GLPoint(_rectangle.Origin.X + _rectangle.Size.Width, _rectangle.Origin.Y + _rectangle.Size.Height));
            return new GLRect(origin, corner);
        }

        public void Drag(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            var offsetY = locD.Y - startD.Y;
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y + offsetY);
        }

    }

    public static class ColorExtensions {
        public static GLColor ToGLColor(this System.Drawing.Color color) {
            return new GLColor(color.A / 255.0, color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }

    }
}
