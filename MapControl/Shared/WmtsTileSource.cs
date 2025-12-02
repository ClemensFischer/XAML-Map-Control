using System;
using System.Collections.Generic;

namespace MapControl
{
    public class WmtsTileSource(WmtsTileMatrixSet tileMatrixSet) : TileSource
    {
        private readonly string uriFormat = tileMatrixSet.UriTemplate
            .Replace("{TileMatrix}", "{0}")
            .Replace("{TileCol}", "{1}")
            .Replace("{TileRow}", "{2}");

        private readonly List<WmtsTileMatrix> tileMatrixes = tileMatrixSet.TileMatrixes;

        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            return zoomLevel < tileMatrixes.Count
                ? new Uri(string.Format(uriFormat, tileMatrixes[zoomLevel].Identifier, column, row))
                : null;
        }
    }
}
