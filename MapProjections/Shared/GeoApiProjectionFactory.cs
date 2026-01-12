using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        public Dictionary<int, string> CoordinateSystemWkts { get; } = [];

        public override MapProjection GetProjection(string crsId) => crsId switch
        {
            MapControl.WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
            MapControl.WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
            MapControl.Wgs84AutoUtmProjection.DefaultCrsId => new Wgs84AutoUtmProjection(),
            _ => base.GetProjection(crsId)
        };

        public override MapProjection GetProjection(int epsgCode) => epsgCode switch
        {
            var code when code >= Ed50UtmProjection.FirstZoneEpsgCode && code <= Ed50UtmProjection.LastZoneEpsgCode => new Ed50UtmProjection(epsgCode % 100),
            var code when code >= Etrs89UtmProjection.FirstZoneEpsgCode && code <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(epsgCode % 100),
            var code when code >= Nad27UtmProjection.FirstZoneEpsgCode && code <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(epsgCode % 100),
            var code when code >= Nad83UtmProjection.FirstZoneEpsgCode && code <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(epsgCode % 100),
            var code when code >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && code <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.North),
            var code when code >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && code <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.South),
            _ => CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt) ? new GeoApiProjection(wkt) : base.GetProjection(epsgCode)
        };
    }
}
