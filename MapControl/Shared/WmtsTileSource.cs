using System;
using System.Collections.Generic;
using System.Text;

namespace MapControl
{
    public class WmtsTileSource : UriTileSource
    {
        private readonly IList<WmtsTileMatrix> tileMatrixes;

        public WmtsTileSource(WmtsTileMatrixSet tileMatrixSet, string uriTemplate)
        {
            tileMatrixes = tileMatrixSet.TileMatrixes;
            UriTemplate = uriTemplate.Replace("{TileMatrixSet}", tileMatrixSet.Identifier);
        }

        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            Uri uri = null;

            if (UriTemplate != null &&
                tileMatrixes != null &&
                tileMatrixes.Count > zoomLevel)
            {
                var uriBuilder = new StringBuilder(UriTemplate);

                uriBuilder.Replace("{TileMatrix}", tileMatrixes[zoomLevel].Identifier);
                uriBuilder.Replace("{TileCol}", column.ToString());
                uriBuilder.Replace("{TileRow}", row.ToString());

                uri = new Uri(uriBuilder.ToString());
            }

            return uri;
        }
    }
}
