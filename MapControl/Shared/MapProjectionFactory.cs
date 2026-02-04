using System;

namespace MapControl
{
    public class MapProjectionFactory
    {
        public MapProjection GetProjection(string crsId)
        {
            var projection = CreateProjection(crsId);

            if (projection == null &&
                crsId.StartsWith("EPSG:") &&
                int.TryParse(crsId.Substring(5), out int epsgCode))
            {
                projection = CreateProjection(epsgCode);
            }

            return projection ?? throw new NotSupportedException($"MapProjection \"{crsId}\" is not supported.");
        }

        protected virtual MapProjection CreateProjection(string crsId)
        {
            MapProjection projection = crsId switch
            {
                WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
                WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
                Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
                Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
                EquirectangularProjection.DefaultCrsId or "CRS:84" => new EquirectangularProjection(crsId),
                _ => null
            };

            if (projection == null && crsId.StartsWith(StereographicProjection.DefaultCrsId))
            {
                projection = new StereographicProjection(crsId);
            }

            return projection;
        }

        protected virtual MapProjection CreateProjection(int epsgCode)
        {
            return epsgCode switch
            {
                var c when c is >= Etrs89UtmProjection.FirstZoneEpsgCode
                            and <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(c % 100),
                var c when c is >= Nad83UtmProjection.FirstZoneEpsgCode
                            and <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(c % 100),
                var c when c is >= Wgs84UtmProjection.FirstZoneNorthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(c % 100, true),
                var c when c is >= Wgs84UtmProjection.FirstZoneSouthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(c % 100, false),
                _ => null
            };
        }
    }
}
