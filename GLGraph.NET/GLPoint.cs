namespace GLGraph.NET {
    public sealed class GLPoint {
        public double X { get; private set; }
        public double Y { get; private set; }

        public GLPoint(double x, double y) {
            X = x;
            Y = y;
        }
    }
}
