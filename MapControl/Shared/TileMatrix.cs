namespace MapControl
{
    public class TileMatrix(int zoomLevel, int xMin, int yMin, int xMax, int yMax)
    {
        public int ZoomLevel { get; } = zoomLevel;
        public int XMin { get; } = xMin;
        public int YMin { get; } = yMin;
        public int XMax { get; } = xMax;
        public int YMax { get; } = yMax;
    }
}
