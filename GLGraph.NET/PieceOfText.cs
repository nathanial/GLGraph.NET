using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GLGraph.NET {
    public class PieceOfText {
        int _texture;
        readonly Font _font;

        public string Text { get; set; }

        readonly int _bmpWidth;
        readonly int _bmpHeight;

        public PieceOfText(Font font, string text) {
            _font = font;
            Text = text;
            _texture = GL.GenTexture();
            Debug.WriteLine("Generating Piece Of Text: " + text);
            var measure = MeasureText();
            using (var bmp = new Bitmap(MakeEven((int)Math.Ceiling(measure.Width)), MakeEven((int)Math.Ceiling(measure.Height)))) {
                using (var g = Graphics.FromImage(bmp)) {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.DrawString(Text, _font, Brushes.Black, new RectangleF(0, 0, measure.Width, measure.Height));
                }

                GL.BindTexture(TextureTarget.Texture2D, _texture);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

                var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                bmp.UnlockBits(data);
                _bmpWidth = bmp.Width;
                _bmpHeight = bmp.Height;
            }
        }

        public void Draw(GLPoint origin, float? glWidth, float? glHeight, bool offsetHeight) {
            float width = glWidth ?? _bmpWidth;
            float height = glHeight ?? _bmpHeight;

            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.PushMatrix();

            if (offsetHeight) {
                GL.Translate(Math.Round(origin.X), Math.Round(origin.Y - _font.Height/2.0), 0);
            } else {
                GL.Translate(Math.Round(origin.X), Math.Round(origin.Y),0);
            }

            GL.Begin(BeginMode.Quads);
            GL.Color3(0,0,0);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0, height); //topleft
            GL.TexCoord2(1f, 0f); GL.Vertex2(width, height);
            GL.TexCoord2(1f, 1f); GL.Vertex2(width, 0);
            GL.TexCoord2(0f, 1f); GL.Vertex2(0, 0);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.PopMatrix();
        }

        int MakeEven(int x) {
            return x%2 != 0 ? x + 1 : x;
        }

        public void Dispose() {
            GL.DeleteTextures(1, ref _texture);
        }

        SizeF MeasureText() {
            using(var bitmap = new Bitmap(200, 200)) {
                using(var g = Graphics.FromImage(bitmap)) {
                    var sf = g.MeasureString(Text, _font,200);
                    return new SizeF(sf.Width, sf.Height);
                }
            }
        }
    }


}
