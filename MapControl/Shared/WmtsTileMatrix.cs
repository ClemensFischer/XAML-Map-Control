using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    public class WmtsTileMatrix(
        string identifier, double scale, Point topLeft, int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
    {
        public string Identifier => identifier;
        public double Scale => scale;
        public Point TopLeft => topLeft;
        public int TileWidth => tileWidth;
        public int TileHeight => tileHeight;
        public int MatrixWidth => matrixWidth;
        public int MatrixHeight => matrixHeight;

        // Indicates if the total width in meters matches the earth circumference.
        //
        public bool HasFullHorizontalCoverage { get; } =
            Math.Abs(matrixWidth * tileWidth / scale - 360d * MapProjection.Wgs84MeterPerDegree) < 1e-3;
    }
}
