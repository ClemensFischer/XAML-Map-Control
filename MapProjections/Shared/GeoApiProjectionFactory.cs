using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        public static GeoApiProjectionFactory GetInstance()
        {
            if (!(Instance is GeoApiProjectionFactory factory))
            {
                factory = new GeoApiProjectionFactory();
                Instance = factory;
            }

            return factory;
        }

        public override MapProjection GetProjection(string crsId)
        {
            switch (crsId)
            {
                case MapControl.WebMercatorProjection.DefaultCrsId:
                    return new WebMercatorProjection();

                case MapControl.WorldMercatorProjection.DefaultCrsId:
                    return new WorldMercatorProjection();

                case MapControl.Wgs84AutoUtmProjection.DefaultCrsId:
                    return new Wgs84AutoUtmProjection();

                default:
                    return base.GetProjection(crsId);
            }
        }

        public override MapProjection GetProjection(int epsgCode)
        {
            switch (epsgCode)
            {
                case int c when c >= Ed50UtmProjection.FirstZoneEpsgCode && c <= Ed50UtmProjection.LastZoneEpsgCode:
                    return new Ed50UtmProjection(epsgCode % 100);

                case var c when c >= Etrs89UtmProjection.FirstZoneEpsgCode && c <= Etrs89UtmProjection.LastZoneEpsgCode:
                    return new Etrs89UtmProjection(epsgCode % 100);

                case var c when c >= Nad27UtmProjection.FirstZoneEpsgCode && c <= Nad27UtmProjection.LastZoneEpsgCode:
                    return new Nad27UtmProjection(epsgCode % 100);

                case var c when c >= Nad83UtmProjection.FirstZoneEpsgCode && c <= Nad83UtmProjection.LastZoneEpsgCode:
                    return new Nad83UtmProjection(epsgCode % 100);

                case var c when c >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && c <= Wgs84UtmProjection.LastZoneNorthEpsgCode:
                    return new Wgs84UtmProjection(epsgCode % 100, true);

                case var c when c >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && c <= Wgs84UtmProjection.LastZoneSouthEpsgCode:
                    return new Wgs84UtmProjection(epsgCode % 100, false);

                default:
                    return CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt)
                        ? new GeoApiProjection(wkt)
                        : base.GetProjection(epsgCode);
            }
        }

        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>();
    }
}
