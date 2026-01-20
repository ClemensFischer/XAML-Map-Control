using System;

namespace MapControl
{
    public class MapProjectionFactory
    {
        public virtual MapProjection GetProjection(string crsId)
        {
            MapProjection projection = crsId switch
            {
                WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
                WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
                EquirectangularProjection.DefaultCrsId or "CRS:84" or "EPSG:4087" => new EquirectangularProjection(crsId),
                Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
                Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
                Wgs84AutoUtmProjection.DefaultCrsId => new Wgs84AutoUtmProjection(),
                Wgs84AutoTmProjection.DefaultCrsId => new Wgs84AutoTmProjection(),
                OrthographicProjection.DefaultCrsId => new OrthographicProjection(),
                AutoEquirectangularProjection.DefaultCrsId => new AutoEquirectangularProjection(),
                GnomonicProjection.DefaultCrsId => new GnomonicProjection(),
                StereographicProjection.DefaultCrsId => new StereographicProjection(),
                AzimuthalEquidistantProjection.DefaultCrsId => new AzimuthalEquidistantProjection(),
                _ => GetProjectionFromEpsgCode(crsId),
            };

            return projection ?? throw new NotSupportedException($"MapProjection \"{crsId}\" is not supported.");
        }

        public MapProjection GetProjectionFromEpsgCode(string crsId) =>
            crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode) ? GetProjection(epsgCode) : null;

        public virtual MapProjection GetProjection(int epsgCode) => epsgCode switch
        {
            var code when code >= Etrs89UtmProjection.FirstZoneEpsgCode && code <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(epsgCode % 100),
            var code when code >= Nad27UtmProjection.FirstZoneEpsgCode && code <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(epsgCode % 100),
            var code when code >= Nad83UtmProjection.FirstZoneEpsgCode && code <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(epsgCode % 100),
            var code when code >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && code <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.North),
            var code when code >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && code <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.South),
            _ => null
        };
    }
}
