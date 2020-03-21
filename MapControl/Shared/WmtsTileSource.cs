// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;

namespace MapControl
{
    public class WmtsTileSource : TileSource
    {
        public WmtsTileSource(string uriFormat)
            : base(uriFormat)
        {
        }

        public WmtsTileMatrixSet TileMatrixSet { get; set; }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (TileMatrixSet != null && zoomLevel >= 0 && zoomLevel < TileMatrixSet.TileMatrixes.Count)
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
