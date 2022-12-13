// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        private const int WorldMercator = 3395;
        private const int WebMercator = 3857;
        private const int Ed50UtmFirst = 23028;
        private const int Ed50UtmLast = 23038;
        private const int Etrs89UtmFirst = 25828;
        private const int Etrs89UtmLast = 25838;
        private const int Wgs84UtmNorthFirst = 32601;
        private const int Wgs84UtmNorthLast = 32660;
        private const int Wgs84UtmSouthFirst = 32701;
        private const int Wgs84UtmSouthLast = 32760;

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
                    case WorldMercator:
                        projection = new WorldMercatorProjection();
                        break;

                    case WebMercator:
                        projection = new WebMercatorProjection();
                        break;

                    case int c when c >= Ed50UtmFirst && c <= Ed50UtmLast:
                        projection = new Ed50UtmProjection(epsgCode % 100);
                        break;

                    case int c when c >= Etrs89UtmFirst && c <= Etrs89UtmLast:
                        projection = new Etrs89UtmProjection(epsgCode % 100);
                        break;

                    case int c when c >= Wgs84UtmNorthFirst && c <= Wgs84UtmNorthLast:
                        projection = new Wgs84UtmProjection(epsgCode % 100, true);
                        break;

                    case int c when c >= Wgs84UtmSouthFirst && c <= Wgs84UtmSouthLast:
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
