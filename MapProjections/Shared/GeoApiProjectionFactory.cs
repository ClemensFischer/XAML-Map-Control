// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>();

        public override MapProjection GetProjection(int epsgCode)
        {
            MapProjection projection = null;

            if (CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt))
            {
                projection = new GeoApiProjection(wkt);
            }
            else
            {
                switch (epsgCode)
                {
                    case WorldMercatorProjection.DefaultEpsgCode:
                        projection = new WorldMercatorProjection();
                        break;

                    case WebMercatorProjection.DefaultEpsgCode:
                        projection = new WebMercatorProjection();
                        break;

                    case int c when c >= Ed50UtmProjection.FirstZoneEpsgCode && c <= Ed50UtmProjection.LastZoneEpsgCode:
                        projection = new Ed50UtmProjection(epsgCode % 100);
                        break;

                    case var c when c >= Etrs89UtmProjection.FirstZoneEpsgCode && c <= Etrs89UtmProjection.LastZoneEpsgCode:
                        projection = new Etrs89UtmProjection(epsgCode % 100);
                        break;

                    case var c when c >= Nad27UtmProjection.FirstZoneEpsgCode && c <= Nad27UtmProjection.LastZoneEpsgCode:
                        projection = new Nad27UtmProjection(epsgCode % 100);
                        break;

                    case var c when c >= Nad83UtmProjection.FirstZoneEpsgCode && c <= Nad83UtmProjection.LastZoneEpsgCode:
                        projection = new Nad83UtmProjection(epsgCode % 100);
                        break;

                    case var c when c >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && c <= Wgs84UtmProjection.LastZoneNorthEpsgCode:
                        projection = new Wgs84UtmProjection(epsgCode % 100, true);
                        break;

                    case var c when c >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && c <= Wgs84UtmProjection.LastZoneSouthEpsgCode:
                        projection = new Wgs84UtmProjection(epsgCode % 100, false);
                        break;

                    default:
                        break;
                }
            }

            return projection ?? base.GetProjection(epsgCode);
        }

        public override MapProjection GetProjection(string crsId)
        {
            MapProjection projection = null;

            switch (crsId)
            {
                case Wgs84AutoUtmProjection.DefaultCrsId:
                    projection = new Wgs84AutoUtmProjection();
                    break;

                default:
                    break;
            }

            return projection ?? base.GetProjection(crsId);
        }
    }
}
