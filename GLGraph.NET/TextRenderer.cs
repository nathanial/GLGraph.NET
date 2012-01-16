using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GLGraph.NET {
    public class PieceOfText {
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public PieceOfText(double x, double y, string text) {
            X = x;
            Y = y;
            Text = text;
        }
    }


    public class GDIOpenGLTextRenderer {
        readonly int _textTexture;
        readonly List<PieceOfText> _piecesOfText = new List<PieceOfText>();
        readonly Font _font;

        public GDIOpenGLTextRenderer() {
            _font = new Font("Arial", 10);
            _textTexture = GL.GenTexture();
        }

        public void AddText(IList<PieceOfText> piecesOfText) {
            _piecesOfText.Clear();
            _piecesOfText.AddRange(piecesOfText);
        }

        public void Draw(int width, int height, GLRectangle rect) {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp)) {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                foreach (var t in _piecesOfText) {
                    g.DrawString(t.Text, _font, Brushes.Black, new PointF((float)t.X, (float)(height - t.Y)));
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, _textTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);


            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            var y0 = rect.TopLeft.Y;
            var y1 = rect.TopRight.Y;
            var y2 = rect.BottomRight.Y;
            var y3 = rect.BottomLeft.Y;

            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f); GL.Vertex2(rect.TopLeft.X, y0);
            GL.TexCoord2(1f, 0f); GL.Vertex2(rect.TopRight.X, y1);
            GL.TexCoord2(1f, 1f); GL.Vertex2(rect.BottomRight.X, y2);
            GL.TexCoord2(0f, 1f); GL.Vertex2(rect.BottomLeft.X, y3);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose() {
            GL.DeleteTexture(_textTexture);
        }

    }
}
