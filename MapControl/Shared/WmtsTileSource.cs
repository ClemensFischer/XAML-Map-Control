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

        public WmtsTileSource(string uriFormat, IList<WmtsTileMatrix> tileMatrixes)
            : base(uriFormat)
        {
            this.tileMatrixes = tileMatrixes;
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (zoomLevel >= 0 && zoomLevel < tileMatrixes.Count)
            {
                uri = new Uri(UriFormat
                    .Replace("{TileMatrix}", tileMatrixes[zoomLevel].Identifier)
                    .Replace("{TileCol}", x.ToString())
                    .Replace("{TileRow}", y.ToString()));
            }

            return uri;
        }
    }
}
