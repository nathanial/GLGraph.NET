using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GLGraph.NET {
    public class PieceOfText {
        int _texture;
        readonly Font _font;

        public string Text { get; set; }

        public PieceOfText(Font font, string text) {
            _font = font;
            Text = text;
            _texture = GL.GenTexture();
        }

        public void Draw(GLPoint origin) {
            var width = 200;
            var height = 50;
            using (var bmp = new Bitmap(width, height)) {
                using (var g = Graphics.FromImage(bmp)) {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.DrawString(Text, _font, Brushes.Black, new PointF(0, 25));
                }

                GL.BindTexture(TextureTarget.Texture2D, _texture);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

                var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.PushMatrix();

            GL.Translate(origin.X, origin.Y, 0);

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0, height); //topleft
            GL.TexCoord2(1f, 0f); GL.Vertex2(width, height);
            GL.TexCoord2(1f, 1f); GL.Vertex2(width, 0);
            GL.TexCoord2(0f, 1f); GL.Vertex2(0, 0);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.PopMatrix();
        }

        public void Dispose() {
            GL.DeleteTextures(1, ref _texture);
        }
    }


}
