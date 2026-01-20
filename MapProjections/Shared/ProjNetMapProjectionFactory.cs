using System.Collections.Generic;

namespace MapControl.Projections
{
    public class ProjNetMapProjectionFactory : MapProjectionFactory
    {
        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>
        {
            { 2056, WktConstants.ProjCsCh1903Lv95 },
            { 2100, WktConstants.ProjCsGgrs87 },
            { 2180, WktConstants.ProjCsEtrf2000Pl },
            { 3034, WktConstants.ProjCsEtrs89LccEurope },
            { 3035, WktConstants.ProjCsEtrs89LaeaEurope },
            { 4087, WktConstants.ProjCsWgs84 },
            { 4647, WktConstants.ProjCsEtrs89Utm32NzEN },
            { 4839, WktConstants.ProjCsEtrs89LccGermanyNE },
            { 5243, WktConstants.ProjCsEtrs89LccGermanyEN },
            { 21781, WktConstants.ProjCsCh1903Lv03 },
            { 29187, WktConstants.ProjCsSad69Utm17S },
            { 29188, WktConstants.ProjCsSad69Utm18S },
            { 29189, WktConstants.ProjCsSad69Utm19S },
            { 29190, WktConstants.ProjCsSad69Utm20S },
            { 29191, WktConstants.ProjCsSad69Utm21S },
            { 29192, WktConstants.ProjCsSad69Utm22S },
            { 29193, WktConstants.ProjCsSad69Utm23S },
        };

        public override MapProjection GetProjection(string crsId)
        {
            MapProjection projection = crsId switch
            {
                MapControl.WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
                MapControl.WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
                MapControl.Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
                MapControl.Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
                MapControl.Wgs84AutoUtmProjection.DefaultCrsId => new Wgs84AutoUtmProjection(),
                MapControl.OrthographicProjection.DefaultCrsId => new Wgs84OrthographicProjection(),
                MapControl.StereographicProjection.DefaultCrsId => new Wgs84StereographicProjection(),
                _ => GetProjectionFromEpsgCode(crsId)
            };

            return projection ?? base.GetProjection(crsId);
        }

        public override MapProjection GetProjection(int epsgCode) => epsgCode switch
        {
            var code when code >= Ed50UtmProjection.FirstZoneEpsgCode && code <= Ed50UtmProjection.LastZoneEpsgCode => new Ed50UtmProjection(epsgCode % 100),
            var code when code >= Etrs89UtmProjection.FirstZoneEpsgCode && code <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(epsgCode % 100),
            var code when code >= Nad27UtmProjection.FirstZoneEpsgCode && code <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(epsgCode % 100),
            var code when code >= Nad83UtmProjection.FirstZoneEpsgCode && code <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(epsgCode % 100),
            var code when code >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && code <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.North),
            var code when code >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && code <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.South),
            _ => CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt) ? new ProjNetMapProjection(wkt) : base.GetProjection(epsgCode)
        };
    }
}
