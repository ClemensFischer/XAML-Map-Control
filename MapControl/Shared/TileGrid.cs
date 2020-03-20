// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class TileGrid
    {
        public readonly int ZoomLevel;
        public readonly int XMin;
        public readonly int YMin;
        public readonly int XMax;
        public readonly int YMax;

        public TileGrid(int zoomLevel, int xMin, int yMin, int xMax, int yMax)
        {
            ZoomLevel = zoomLevel;
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }
    }
}
