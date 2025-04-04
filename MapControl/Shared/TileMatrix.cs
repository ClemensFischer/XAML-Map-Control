﻿namespace MapControl
{
    public class TileMatrix
    {
        public TileMatrix(int zoomLevel, int xMin, int yMin, int xMax, int yMax)
        {
            ZoomLevel = zoomLevel;
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public int ZoomLevel { get; }
        public int XMin { get; }
        public int YMin { get; }
        public int XMax { get; }
        public int YMax { get; }
    }
}
