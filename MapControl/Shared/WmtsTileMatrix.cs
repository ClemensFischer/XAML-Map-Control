// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    public class WmtsTileMatrix
    {
        // See 07-057r7_Web_Map_Tile_Service_Standard.pdf, section 6.1.a, page 8:
        // "standardized rendering pixel size" is 0.28 mm

        public WmtsTileMatrix(string identifier, double scaleDenominator, Point topLeft,
            int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
        {
            Identifier = identifier;
            Scale = 1 / (scaleDenominator * 0.00028); // 0.28 mm
            TopLeft = topLeft;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            MatrixWidth = matrixWidth;
            MatrixHeight = matrixHeight;
        }

        public string Identifier { get; }
        public double Scale { get; }
        public Point TopLeft { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int MatrixWidth { get; }
        public int MatrixHeight { get; }
    }
}
