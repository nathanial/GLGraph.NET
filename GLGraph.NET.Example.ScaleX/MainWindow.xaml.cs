using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GLGraph.NET.Extensions;

namespace GLGraph.NET.Example.ScaleX {
    public partial class MainWindow {
        LineGraph _graph;

        public MainWindow() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += delegate {
                _graph = new LineGraph();
                host.Child = _graph.Control;
                ShowScaledGraph();
            };
        }

        void ShowScaledGraph() {
            _graph.Display(new GLRect(-50,-50,100,100), true);

            var random = new Random();
            var dataInMilliseconds = new GLPoint[] {
                new GLPoint(0, 0), 
                new GLPoint(3000, random.NextDouble()), 
                new GLPoint(6000, random.NextDouble()), 
                new GLPoint(9000, random.NextDouble()), 
                new GLPoint(12000, random.NextDouble()), 
                new GLPoint(15000, random.NextDouble()), 
                new GLPoint(18000, random.NextDouble()), 
                new GLPoint(21000, random.NextDouble()), 
            };

            var dataInSeconds = dataInMilliseconds.Select(p => new GLPoint(p.X/1000, p.Y)).ToArray();



            _graph.Lines.Add(new Line(1.0f, System.Drawing.Color.Blue.ToGLColor(), dataInSeconds));
        }
    }
}
