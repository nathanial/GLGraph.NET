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
            _pos = new PieceOfText(new Font("Courier", 12), Text);
        }

        public void Draw(GraphWindow window) {
            _pos.Draw(new GLPoint(X,Y), (float) (window.DataWidth / 10.0), (float) (window.DataHeight / 10.0),false);
        }
    }
}
