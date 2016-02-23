// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class TileGrid : IEquatable<TileGrid>
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

        public bool Equals(TileGrid tileGrid)
        {
            return ReferenceEquals(this, tileGrid)
                || (tileGrid != null
                && tileGrid.ZoomLevel == ZoomLevel
                && tileGrid.XMin == XMin
                && tileGrid.YMin == YMin
                && tileGrid.XMax == XMax
                && tileGrid.YMax == YMax);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TileGrid);
        }

        public override int GetHashCode()
        {
            return ZoomLevel ^ XMin ^ YMin ^ XMax ^ YMax;
        }
    }
}
