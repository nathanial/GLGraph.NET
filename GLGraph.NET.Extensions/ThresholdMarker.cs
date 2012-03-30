using System;
using System.Drawing;

namespace GLGraph.NET.Extensions {
    public class ThresholdMarker : IDrawable {
        readonly Rectangle _rectangle;
        const double EdgeThreshold = 3;

        public ThresholdMarker(GLPoint location, GLSize size, GLColor color) {
            _rectangle = new Rectangle(color, true, location, size);
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X - size.Width / 2.0, _rectangle.Origin.Y - size.Height / 2.0);
        }

        public void Draw(GraphWindow window) {
            _rectangle.Draw();
        }

        public HitKind HitTest(GraphWindow window, Point wloc) {
            var spos = ScreenPosition(window);
            if (LeftEdgeTest(spos, wloc)) return HitKind.LeftEdge;
            if (TopEdgeTest(spos, wloc)) return HitKind.TopEdge;
            if (BottomEdgeTest(spos, wloc)) return HitKind.BottomEdge;
            if (RightEdgeTest(spos, wloc)) return HitKind.RightEdge;
            if (CenterTest(spos, wloc)) return HitKind.Center;
            return HitKind.None;
        }

        public void Drag(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            var offsetY = locD.Y - startD.Y;
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y + offsetY);
        }


        public void ResizeLeft(GraphWindow window, Point start, Point location) {
            var locD = window.ScreenToView(new GLPoint(location.X, location.Y));
            var startD = window.ScreenToView(new GLPoint(start.X, start.Y));
            var offsetX = locD.X - startD.X;
            _rectangle.Origin = new GLPoint(_rectangle.Origin.X + offsetX, _rectangle.Origin.Y);
            _rectangle.Size = new GLSize(_rectangle.Size.Width - offsetX, _rectangle.Size.Height);
        }


        public void ResizeRight(GraphWindow window, Point start, Point location) {
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

    public enum HitKind {
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
