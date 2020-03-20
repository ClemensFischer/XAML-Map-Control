// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;

namespace MapControl
{
    public class WmtsTileSource : TileSource
    {
        private readonly IList<WmtsTileMatrix> tileMatrixes;

        public WmtsTileSource(string uriFormat, WmtsTileMatrixSet tileMatrixSet)
            : base(uriFormat.Replace("{TileMatrixSet}", tileMatrixSet.Identifier))
        {
            tileMatrixes = tileMatrixSet.TileMatrixes;
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel < 0 || zoomLevel >= tileMatrixes.Count)
            {
                return null;
            }

            var url = UriFormat
                .Replace("{TileMatrix}", tileMatrixes[zoomLevel].Identifier)
                .Replace("{TileCol}", x.ToString())
                .Replace("{TileRow}", y.ToString());

            return new Uri(url);
        }
    }
}
