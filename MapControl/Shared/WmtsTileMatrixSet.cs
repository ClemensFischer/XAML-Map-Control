using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    public class WmtsTileMatrixSet(
        string identifier, string supportedCrsId, string uriTemplate, IEnumerable<WmtsTileMatrix> tileMatrixes)
    {
        public string Identifier => identifier;
        public string SupportedCrsId => supportedCrsId;
        public string UriTemplate { get; } = uriTemplate.Replace("{TileMatrixSet}", identifier);
        public List<WmtsTileMatrix> TileMatrixes { get; } = tileMatrixes.OrderBy(m => m.Scale).ToList();

        public static WmtsTileMatrixSet CreateOpenStreetMapTileMatrixSet(
            string uriTemplate, int minZoomLevel = 0, int maxZoomLevel = 19)
        {
            const int tileSize = 256;
            const double baseScale = tileSize / (360d * MapProjection.Wgs84MeterPerDegree);

            Point mapTopLeft = new(-180d * MapProjection.Wgs84MeterPerDegree,
                                    180d * MapProjection.Wgs84MeterPerDegree);

            return new WmtsTileMatrixSet(null,
                WebMercatorProjection.DefaultCrsId,
                uriTemplate
                    .Replace("{z}", "{0}")
                    .Replace("{x}", "{1}")
                    .Replace("{y}", "{2}"),
                Enumerable
                    .Range(minZoomLevel, maxZoomLevel - minZoomLevel + 1)
                    .Select<int, (int zoomLevel, int matrixSize)>(z => (z, 1 << z))
                    .Select(t => new WmtsTileMatrix(
                        t.zoomLevel.ToString(),
                        t.matrixSize * baseScale, mapTopLeft,
                        tileSize, tileSize, t.matrixSize, t.matrixSize)));
        }
    }
}
