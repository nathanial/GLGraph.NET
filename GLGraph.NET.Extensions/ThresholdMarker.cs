using System;
using System.Drawing;
using System.Windows.Forms;

namespace GLGraph.NET.Extensions {

    enum DragMode {
        None,
        ResizeLeft,
        ResizeRight,
        Center
    }

    public class ThresholdMarker : IDrawable {
        readonly Rectangle _rectangle;
        readonly ILineGraph _graph;
        const double EdgeThreshold = 3;

        static readonly Cursor Hand = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_hand.png"), 8, 8);
        static readonly Cursor HandDrag = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_drag_hand.png"), 8, 8);

        DragMode _hypotheticalDragMode;
        DragMode _dragMode;
        Point? _dragStart;

        public ThresholdMarker(ILineGraph graph, GLPoint location, GLSize size, GLColor color) {
            _graph = graph;
            _rectangle = new Rectangle(color, true, location, size);
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X - size.Width / 2.0, _rectangle.Origin.Y - size.Height / 2.0);

            graph.Control.MouseMove += OnMouseMove;
            graph.Control.MouseDown += OnMouseDown;
            graph.Control.MouseUp += OnMouseUp;
        }

        public void Draw(GraphWindow window) {
            _rectangle.Draw();
        }

        HitKind HitTest(GraphWindow window, Point wloc) {
            var spos = ScreenPosition(window);
            if (LeftEdgeTest(spos, wloc)) return HitKind.LeftEdge;
            if (TopEdgeTest(spos, wloc)) return HitKind.TopEdge;
            if (BottomEdgeTest(spos, wloc)) return HitKind.BottomEdge;
            if (RightEdgeTest(spos, wloc)) return HitKind.RightEdge;
            if (CenterTest(spos, wloc)) return HitKind.Center;
            return HitKind.None;
        }

        void Drag(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            var offsetY = locD.Y - startD.Y;
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y + offsetY);
        }

        void ResizeLeft(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y);
            _rectangle.Size = new GLSize(_rectangle.Size.Width - offsetX, _rectangle.Size.Height);
        }

        void ResizeRight(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            _rectangle.Size = new GLSize(_rectangle.Size.Width + offsetX, _rectangle.Size.Height);
        }

        GLRect ScreenPosition(GraphWindow window) {
            var origin = window.ViewToScreen(_rectangle.Origin);
            var corner = window.ViewToScreen(new GLPoint(_rectangle.Origin.X + _rectangle.Size.Width, _rectangle.Origin.Y + _rectangle.Size.Height));
            return new GLRect(origin, corner);
        }

        void OnMouseMove(object sender, MouseEventArgs args) {
            switch (_dragMode) {
                case DragMode.None:
                    var hit = HitTest(_graph.Window, args.Location);
                    switch (hit) {
                        case HitKind.None:
                            _graph.Control.Cursor = Cursors.Default;
                            _dragMode = DragMode.None;
                            _hypotheticalDragMode = DragMode.None;
                            _dragStart = null;
                            break;
                        case HitKind.LeftEdge:
                            _graph.Control.Cursor = Cursors.SizeWE;
                            _hypotheticalDragMode = DragMode.ResizeLeft;
                            break;
                        case HitKind.RightEdge:
                            _graph.Control.Cursor = Cursors.SizeWE;
                            _hypotheticalDragMode = DragMode.ResizeRight;
                            break;
                        case HitKind.Center:
                            _graph.Control.Cursor = Hand;
                            _hypotheticalDragMode = DragMode.Center;
                            break;
                    }
                    break;
                case DragMode.ResizeLeft:
                    ResizeLeft(_graph.Window, _dragStart.Value, args.Location);
                    _dragStart = args.Location;
                    _graph.Draw();
                    break;
                case DragMode.ResizeRight:
                    ResizeRight(_graph.Window, _dragStart.Value, args.Location);
                    _dragStart = args.Location;
                    _graph.Draw();
                    break;
                case DragMode.Center:
                    Drag(_graph.Window, _dragStart.Value, args.Location);
                    _dragStart = args.Location;
                    _graph.Draw();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        void OnMouseDown(object sender, MouseEventArgs args) {
            if (args.Button == MouseButtons.Left) {
                _dragMode = _hypotheticalDragMode;
                _dragStart = args.Location;
                if (_dragMode != DragMode.None) {
                    _graph.PanningIsEnabled = false;
                }
                if (_dragMode == DragMode.Center) {
                    _graph.Control.Cursor = HandDrag;
                }
            }
        }

        void OnMouseUp(object sender, MouseEventArgs mouseEventArgs) {
            _graph.PanningIsEnabled = true;
            _dragMode = DragMode.None;
            _dragStart = null;
            _hypotheticalDragMode = DragMode.None;
            _graph.Control.Cursor = Cursors.Default;
        }

        static bool RightEdgeTest(GLRect marker, Point pt) {
            var dist = Math.Abs((marker.X + marker.Width) - pt.X);
            return marker.ContainsY(pt.Y) && dist < EdgeThreshold;
        }

        static bool BottomEdgeTest(GLRect marker, Point pt) {
            var dist = Math.Abs(marker.Y - pt.Y);
            return marker.ContainsX(pt.X) && dist < EdgeThreshold;
        }

        static bool TopEdgeTest(GLRect marker, Point pt) {
            var dist = Math.Abs((marker.Y + marker.Height) - pt.Y);
            return marker.ContainsX(pt.X) && dist < EdgeThreshold;
        }

        static bool LeftEdgeTest(GLRect marker, Point pt) {
            var dist = Math.Abs(marker.X - pt.X);
            return marker.ContainsY(pt.Y) && dist < EdgeThreshold;
        }

        static bool CenterTest(GLRect marker, Point wloc) {
            return marker.Contains(wloc.X, wloc.Y);
        }
    }

    enum HitKind {
        None,
        LeftEdge,
        TopEdge,
        BottomEdge,
        RightEdge,
        Center
    }



    public static class ColorExtensions {
        public static GLColor ToGLColor(this Color color) {
            return new GLColor(color.A / 255.0, color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
    }

    public static class MathEx {
        public static double Distance(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
        }
    }

}
