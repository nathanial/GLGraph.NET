using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using SharpGL;
using Point = System.Windows.Point;

namespace GLGraph.NET {

    public class PersistentTexture  {
        readonly uint _texture;
        readonly Bitmap _bitmap;

        public PersistentTexture(OpenGL gl, Uri uri) {
            var buf  = new uint[1];
            gl.GenTextures(1, buf);
            _texture = buf[0];

            var stream = Application.GetResourceStream(uri).Stream;
            _bitmap = new Bitmap(stream);
            stream.Close();
            var data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _texture);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA, _bitmap.Width, _bitmap.Height, 0,
                          OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, data.Scan0);
            gl.Flush();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            _bitmap.UnlockBits(data);
        }


        public void Draw(OpenGL gl, GraphWindow window, Point location) {
            gl.Color(1.0, 1.0, 1.0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _texture);

            var height = new Point(0, _bitmap.Height).ToView(window).Y;
            var width = new Point(_bitmap.Width, 0).ToView(window).X;

            var ox = location.X - (width / 2.0);
            var oy = location.Y - (height / 2.0);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0f, 0f); gl.Vertex(ox, oy + height);
            gl.TexCoord(1f, 0f); gl.Vertex(ox + width, oy + height);
            gl.TexCoord(1f, 1f); gl.Vertex(ox + width, oy);
            gl.TexCoord(0f, 1f); gl.Vertex(ox, oy);
            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        public void Dispose(OpenGL gl) {
            _bitmap.Dispose();
            gl.DeleteTextures(1, new uint[1]{_texture});
        }
    }
}