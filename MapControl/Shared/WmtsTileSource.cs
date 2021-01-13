// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class WmtsTileSource : TileSource
    {
        public WmtsTileMatrixSet TileMatrixSet { get; set; }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (UriFormat != null &&
                TileMatrixSet != null &&
                zoomLevel >= 0 &&
                zoomLevel < TileMatrixSet.TileMatrixes.Count)
            {
                uri = new Uri(UriFormat
                    .Replace("{TileMatrixSet}", TileMatrixSet.Identifier)
                    .Replace("{TileMatrix}", TileMatrixSet.TileMatrixes[zoomLevel].Identifier)
                    .Replace("{TileCol}", x.ToString())
                    .Replace("{TileRow}", y.ToString()));
            }

            return uri;
        }
    }
}
