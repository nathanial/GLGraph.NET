using System;
using OpenTK.Graphics.OpenGL;

namespace GLGraph.NET {
    public static class OpenGL {
        public static void DrawQuad(GLPoint topLeft, GLPoint topRight, GLPoint bottomRight, GLPoint bottomLeft) {
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(topLeft.X, topLeft.Y);
            GL.Vertex2(topRight.X, topRight.Y);
            GL.Vertex2(bottomRight.X, bottomRight.Y);
            GL.Vertex2(bottomLeft.X, bottomLeft.Y);
            GL.End();
        }

        public static void DrawQuad(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4) {
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.Vertex2(x3, y3);
            GL.Vertex2(x4, y4);
            GL.End();
        }

        public static void DrawQuad(params double[] points) {
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(points[0], points[1]);
            GL.Vertex2(points[2], points[3]);
            GL.Vertex2(points[4], points[5]);
            GL.Vertex2(points[6], points[7]);
            GL.End();
        }

        public static void DrawLines(params double[] points) {
            GL.Begin(BeginMode.Lines);
            for (var i = 0; i < points.Length; i += 2) {
                GL.Vertex2(points[i], points[i + 1]);
            }
            GL.End();
        }

        public static void DrawVertices(GLPoint[] points) {
            foreach (var p in points) {
                GL.Vertex2(p.X, p.Y);
            }
        }

        public static void DrawMany(int start, int finish, int step, Func<int, GLPoint[]> fun) {
            DrawVertices(Functions.SelectOverMany(start, finish, step, fun));
        }

        public static void PushMatrix(Action action) {
            GL.PushMatrix();
            action();
            GL.PopMatrix();
        }

        public static void Begin(BeginMode mode, Action action) {
            GL.Begin(mode);
            action();
            GL.End();
        }

        public static void WithoutSmoothing(Action action) {
            int oldWidth;
            GL.GetInteger(GetPName.LineWidth, out oldWidth);
            GL.LineWidth(1.0f);
            GL.Disable(EnableCap.LineSmooth);
            action();
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(oldWidth);
        }
    }
}