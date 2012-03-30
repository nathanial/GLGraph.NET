using System;
using System.Drawing;
using System.Windows.Forms;

namespace GLGraph.NET.Extensions {
    public class ThresholdMarker : IDrawable {
        readonly Rectangle _rectangle;
        readonly ILineGraph _graph;
        const double EdgeThreshold = 3;

        bool _dragging;
        HitKind _theHit;
        Point? _dragStart;

        static readonly Cursor Hand = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_hand.png"), 8, 8);
        static readonly Cursor HandDrag = CustomCursor.CreateCursor((Bitmap)Image.FromFile("Cursors\\cursor_drag_hand.png"), 8, 8);

        public ThresholdMarker(ILineGraph graph, GLPoint location, GLSize size, GLColor color) {
            _graph = graph;
            _rectangle = new Rectangle(color, true, location, size);
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X - size.Width / 2.0, _rectangle.Origin.Y - size.Height / 2.0);

            graph.Control.MouseMove += OnMouseMove;
            graph.Control.MouseDown += OnMouseDown;
            graph.Control.MouseUp += ControlOnMouseUp;
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
            if (_dragging) {
                if (_dragStart == null) return;
                switch (_theHit) {
                    case HitKind.None:
                    case HitKind.BottomEdge:
                    case HitKind.TopEdge:
                        break;

                    case HitKind.LeftEdge:
                        ResizeLeft(_graph.Window, _dragStart.Value, args.Location);
                        _dragStart = args.Location;
                        _graph.Draw();
                        break;
                    case HitKind.RightEdge:
                        ResizeRight(_graph.Window, _dragStart.Value, args.Location);
                        _dragStart = args.Location;
                        _graph.Draw();
                        break;

                    case HitKind.Center:
                        Drag(_graph.Window, _dragStart.Value, args.Location);
                        _dragStart = new Point(args.Location.X, args.Location.Y);
                        _graph.Draw();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } else {
                if (_dragging) return;
                var hit = HitTest(_graph.Window, args.Location);
                switch (hit) {
                    case HitKind.LeftEdge:
                    case HitKind.RightEdge:
                        _graph.Control.Cursor = Cursors.SizeWE;
                        _theHit = hit;
                        break;

                    case HitKind.TopEdge:
                    case HitKind.BottomEdge:
                        //do nothing
                        break;

                    case HitKind.Center:
                        _graph.Control.Cursor = Hand;
                        _theHit = hit;
                        break;

                    case HitKind.None:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        void OnMouseDown(object sender, MouseEventArgs args) {
            if (args.Button == MouseButtons.Left) {
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
                        _graph.Control.Cursor = HandDrag;
                        _dragging = true;
                        _dragStart = args.Location;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        void ControlOnMouseUp(object sender, MouseEventArgs mouseEventArgs) {
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
                    _graph.Control.Cursor = Hand;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
