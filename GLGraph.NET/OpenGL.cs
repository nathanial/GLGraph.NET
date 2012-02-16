using System;
using System.Windows;
using SharpGL;

namespace GLGraph.NET {
    public static class OpenGLExtensions {
        public static void DrawQuad(this OpenGL gl, Point topLeft, Point topRight, Point bottomRight, Point bottomLeft) {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(topLeft.X, topLeft.Y);
            gl.Vertex(topRight.X, topRight.Y);
            gl.Vertex(bottomRight.X, bottomRight.Y);
            gl.Vertex(bottomLeft.X, bottomLeft.Y);
            gl.End();
        }

        public static void DrawQuad(this OpenGL gl, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4) {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(x1, y1);
            gl.Vertex(x2, y2);
            gl.Vertex(x3, y3);
            gl.Vertex(x4, y4);
            gl.End();
        }

        public static void DrawQuad(this OpenGL gl, params double[] points) {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex(points[0], points[1]);
            gl.Vertex(points[2], points[3]);
            gl.Vertex(points[4], points[5]);
            gl.Vertex(points[6], points[7]);
            gl.End();
        }

        public static void DrawLines(this OpenGL gl, params double[] points) {
            gl.Begin(OpenGL.GL_LINES);
            for (var i = 0; i < points.Length; i += 2) {
                gl.Vertex(points[i], points[i + 1]);
            }
            gl.End();
        }

        public static void DrawVertices(this OpenGL gl, Point[] points) {
            foreach (var p in points) {
                gl.Vertex(p.X, p.Y);
            }
        }

        public static void DrawMany(this OpenGL gl, int start, int finish, int step, Func<int, Point[]> fun) {
            gl.DrawVertices(Functions.SelectOverMany(start, finish, step, fun));
        }
    }
}