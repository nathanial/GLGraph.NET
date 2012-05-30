using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;

namespace GLGraph.NET.Example.MarkerTest {
    public partial class Form1 : Form {
        readonly LineGraph _graph;

        public Form1() {
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _graph = new LineGraph();
            _graph.TextEnabled = true;
            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                ShowTextGraph();
            };
        }

        void ShowTextGraph() {
           _graph.Markers.Add(new GraphText(10,10,"Hello"));
        }
    }

    class GraphText : IDrawable {
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }

        readonly PieceOfText _pos;
        
        public GraphText(int x, int y, string text) {
            X = x;
            Y = y;
            Text = text;
            _pos = new PieceOfText(new Font("Courier", 32), Text);
        }

        public void Draw(GraphWindow window) {
            var width = (window.DataWidth/window.WindowWidth)*100;
            var height = (window.DataHeight/window.WindowHeight)*50;
            _pos.Draw(new GLPoint(X,Y), (float) width,(float)height,false);
        }

        public void Dispose() {
            _pos.Dispose();
        }
    }
}
