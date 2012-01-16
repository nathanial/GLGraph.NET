using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Point = System.Windows.Point;

namespace GLGraph.NET {

    public class PersistentTexture : IDisposable {
        readonly int _texture;
        readonly Bitmap _bitmap;

        public PersistentTexture(Uri uri) {
            _texture = GL.GenTexture();
            var stream = Application.GetResourceStream(uri).Stream;
            _bitmap = new Bitmap(stream);
            stream.Close();
            var data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmap.Width, _bitmap.Height, 0,
                          PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.Flush();
            GL.BindTexture(TextureTarget.Texture2D, 0);

            _bitmap.UnlockBits(data);
        }


        public void Draw(GraphWindow window, Point location) {
            GL.Color3(1.0, 1.0, 1.0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            var height = new Point(0, _bitmap.Height).ToView(window).Y;
            var width = new Point(_bitmap.Width, 0).ToView(window).X;

            var ox = location.X - (width / 2.0);
            var oy = location.Y - (height / 2.0);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(ox, oy + height);
            GL.TexCoord2(1f, 0f); GL.Vertex2(ox + width, oy + height);
            GL.TexCoord2(1f, 1f); GL.Vertex2(ox + width, oy);
            GL.TexCoord2(0f, 1f); GL.Vertex2(ox, oy);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose() {
            _bitmap.Dispose();
            GL.DeleteTexture(_texture);
        }
    }
}