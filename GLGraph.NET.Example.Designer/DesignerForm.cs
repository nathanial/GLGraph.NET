using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GLGraph.NET.Extensions;

namespace GLGraph.NET.Example.Designer {
    public partial class DesignerForm : Form {
        readonly LineGraph _graph;
        bool _once;

        public DesignerForm() {
            InitializeComponent();
            if(DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _graph = new LineGraph();
            _graph.Control.Dock = DockStyle.Fill;

            Controls.Add(_graph.Control);

            Load += delegate {
                if(!_once) {
                    _once = true;
                    ShowStaticGraph();
                }
            };

        }

        void ShowStaticGraph() {
            var data = new List<GLPoint>();
            var random = new Random();
            for (var i = 0; i < 100; i++) {
                data.Add(new GLPoint(i, random.NextDouble() * 30 - 15));
            }
            _graph.Lines.Add(new Line(1.0f, Color.Black.ToGLColor(), data.ToArray()));
            _graph.Markers.Add(new ThresholdMarker(_graph,new GLPoint(30,10), new GLSize(30,2), Color.Green.ToGLColor()));
            _graph.Display(new GLRect(0, -20, 120, 50), true);
        }
    }
}
