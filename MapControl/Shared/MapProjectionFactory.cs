using System;

namespace MapControl
{
    public class MapProjectionFactory
    {
        public virtual MapProjection GetProjection(string crsId)
        {
            var projection = crsId switch
            {
                WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
                WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
                EquirectangularProjection.DefaultCrsId or "CRS:84" => new EquirectangularProjection(crsId),
                Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
                Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
                Wgs84AutoUtmProjection.DefaultCrsId => new Wgs84AutoUtmProjection(),
                Wgs84AutoTmProjection.DefaultCrsId => new Wgs84AutoTmProjection(),
                OrthographicProjection.DefaultCrsId => new OrthographicProjection(),
                GnomonicProjection.DefaultCrsId => new GnomonicProjection(),
                StereographicProjection.DefaultCrsId => new StereographicProjection(),
                AzimuthalEquidistantProjection.DefaultCrsId => new AzimuthalEquidistantProjection(),
                _ => GetProjectionFromEpsgCode(crsId),
            };

            return projection ?? throw new NotSupportedException($"MapProjection \"{crsId}\" is not supported.");
        }

        public virtual MapProjection GetProjection(int epsgCode)
        {
            return epsgCode switch
            {
                var c when c is >= Etrs89UtmProjection.FirstZoneEpsgCode
                            and <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(c % 100),
                var c when c is >= Nad27UtmProjection.FirstZoneEpsgCode
                            and <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(c % 100),
                var c when c is >= Nad83UtmProjection.FirstZoneEpsgCode
                            and <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(c % 100),
                var c when c is >= Wgs84UtmProjection.FirstZoneNorthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(c % 100, Hemisphere.North),
                var c when c is >= Wgs84UtmProjection.FirstZoneSouthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(c % 100, Hemisphere.South),
                _ => null
            };
        }

        protected MapProjection GetProjectionFromEpsgCode(string crsId)
        {
            return crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode) ? GetProjection(epsgCode) : null;
        }
    }
}
