using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using GLGraph.NET.Example.StressTest;
using GLGraph.NET.Extensions;

namespace GLGraph.NET.Example.WPF.StressTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        LineGraph _graph;
        public MainWindow() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += delegate {
                _graph = new LineGraph();
                host.Child = _graph.Control;
                ShowStaticGraph();
            };
        }

        DispatcherTimer _drawTimer;
        DispatcherTimer _clearTimer;
        DispatcherTimer _rectangleDrawer;
        DispatcherTimer _markerDrawer;

        void ShowStaticGraph() {
            var textures = new Form1.PersistentTextures();
            _graph.Display(new GLRect(0, -20, 1000, 50), true);
            var random = new Random();

            _clearTimer = new DispatcherTimer();
            _clearTimer.Interval = TimeSpan.FromMilliseconds(100);
            _clearTimer.Tick += delegate {
                var count = _graph.Lines.Count;
                if (count > 0) {
                    for (var i = 0; i < count / 2; i++) {
                        _graph.Lines.RemoveAt(0);
                    }
                }
                count = _graph.Markers.Count;
                if (count > 0) {
                    for (var i = 0; i < count / 2; i++) {
                        _graph.Markers.RemoveAt(0);
                    }
                }
                _graph.Draw();
            };
            _clearTimer.Start();

            _drawTimer = new DispatcherTimer();
            _drawTimer.Interval = TimeSpan.FromMilliseconds(10);
            _drawTimer.Tick += delegate {
                var data = new List<GLPoint>();
                for (var i = 0; i < 1000; i++) {
                    data.Add(new GLPoint(i, random.NextDouble() * 30 - 15));
                }
                _graph.Lines.Add(new Line(1.0f, System.Drawing.Color.Black.ToGLColor(), data.ToArray()));
                _graph.Draw();
            };
            _drawTimer.Start();

            _rectangleDrawer = new DispatcherTimer();
            _rectangleDrawer.Interval = TimeSpan.FromMilliseconds(10);
            _rectangleDrawer.Tick += delegate {
                for (var i = 0; i < 10; i++) {
                    _graph.Markers.Add(new Rectangle(new GLColor(1, 0.5, 0.5, 0.5), true,
                                                     new GLPoint(random.NextDouble() * 1000,
                                                                 random.NextDouble() * 30 - 15),
                                                     new GLSize(random.NextDouble() * 10, 1)));
                }
            };
            _rectangleDrawer.Start();

            _markerDrawer = new DispatcherTimer();
            _markerDrawer.Interval = TimeSpan.FromMilliseconds(10);
            _markerDrawer.Tick += delegate {
                for (var i = 0; i < 10; i++) {
                    _graph.Markers.Add(
                        new Form1.SatisfiedMarker(
                            new GLPoint(random.NextDouble() * 1000, random.NextDouble() * 30 - 15),
                            textures.RedOrb));
                }
            };
            _markerDrawer.Start();

        }
    }
}
