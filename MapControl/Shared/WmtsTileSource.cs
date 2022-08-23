// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
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

            if (UriTemplate != null &&
                TileMatrixSet != null && TileMatrixSet.TileMatrixes.Count > zoomLevel &&
                x >= 0 && y >= 0 && zoomLevel >= 0)
            {
                uri = new Uri(UriTemplate
                    .Replace("{TileMatrixSet}", TileMatrixSet.Identifier)
                    .Replace("{TileMatrix}", TileMatrixSet.TileMatrixes[zoomLevel].Identifier)
                    .Replace("{TileCol}", x.ToString())
                    .Replace("{TileRow}", y.ToString()));
            }

            return uri;
        }
    }
}
