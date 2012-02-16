using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL;

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
        readonly uint _textTexture;
        readonly List<PieceOfText> _piecesOfText = new List<PieceOfText>();
        readonly Font _font;

        public GDIOpenGLTextRenderer(OpenGL gl) {
            _font = new Font("Arial", 10);
            var buf = new uint[1];
            gl.GenTextures(1, buf);
            _textTexture = buf[0];
        }

        public void AddText(IList<PieceOfText> piecesOfText) {
            _piecesOfText.Clear();
            _piecesOfText.AddRange(piecesOfText);
        }

        public void Draw(OpenGL gl, int width, int height, GLRectangle rect) {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp)) {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                foreach (var t in _piecesOfText) {
                    g.DrawString(t.Text, _font, Brushes.Black, new PointF((float)t.X, (float)(height - t.Y)));
                }
            }

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _textTexture);

            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);


            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA, width, height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, data.Scan0);

            bmp.UnlockBits(data);

            var y0 = rect.TopLeft.Y;
            var y1 = rect.TopRight.Y;
            var y2 = rect.BottomRight.Y;
            var y3 = rect.BottomLeft.Y;

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0f, 0f); gl.Vertex(rect.TopLeft.X, y0);
            gl.TexCoord(1f, 0f); gl.Vertex(rect.TopRight.X, y1);
            gl.TexCoord(1f, 1f); gl.Vertex(rect.BottomRight.X, y2);
            gl.TexCoord(0f, 1f); gl.Vertex(rect.BottomLeft.X, y3);
            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        public void Dispose(OpenGL gl) {
            gl.DeleteTextures(1, new uint[]{_textTexture});
        }

    }
}
