using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace GLGraph.NET.Example {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            graph.Lines.Add(new Line(1.0f, Colors.Black, new[] { 
                new Point(0,0),
                new Point(1,1),
                new Point(2,0), 
                new Point(3,1), 
                new Point(4,0), 
            }));
            graph.Display(new Rect(0, -2, 10, 4));
            graph.Draw();
        }
    }
}
