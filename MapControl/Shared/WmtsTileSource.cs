using System;
using System.Text;

namespace MapControl
{
    public class WmtsTileSource : TileSource
    {
        public WmtsTileMatrixSet TileMatrixSet { get; set; }

        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            Uri uri = null;

            if (UriTemplate != null &&
                TileMatrixSet != null &&
                TileMatrixSet.TileMatrixes.Count > zoomLevel)
            {
                var uriBuilder = new StringBuilder(UriTemplate);

                uriBuilder.Replace("{TileMatrixSet}", TileMatrixSet.Identifier);
                uriBuilder.Replace("{TileMatrix}", TileMatrixSet.TileMatrixes[zoomLevel].Identifier);
                uriBuilder.Replace("{TileCol}", column.ToString());
                uriBuilder.Replace("{TileRow}", row.ToString());

                uri = new Uri(uriBuilder.ToString());
            }

            return uri;
        }
    }
}
