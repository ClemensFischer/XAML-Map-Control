namespace MapControl
{
    public class TileMatrix(int zoomLevel, int xMin, int yMin, int xMax, int yMax)
    {
        public int ZoomLevel => zoomLevel;
        public int XMin => xMin;
        public int YMin => yMin;
        public int XMax => xMax;
        public int YMax => yMax;
        public int Width => xMax - xMin + 1;
        public int Height => yMax - yMin + 1;
    }
}
