namespace GLGraph.NET {
    public class GLSize {
        readonly double _width;
        readonly double _height;

        public GLSize(double width, double height) {
            _width = width;
            _height = height;
        }

        public double Width { get { return _width; } }
        public double Height { get { return _height; } }
    }
}