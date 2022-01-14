﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    public class WmtsTileMatrix
    {
        public WmtsTileMatrix(string identifier, double scaleDenominator, Point topLeft,
            int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
        {
            Identifier = identifier;
            Scale = 1 / (scaleDenominator * 0.00028);
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
