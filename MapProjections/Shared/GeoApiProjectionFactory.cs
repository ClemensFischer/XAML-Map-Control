// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// � 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        public const int WorldMercator = 3395;
        public const int WebMercator = 3857;
        public const int AutoUtm = 42001;
        public const int Ed50UtmFirst = 23028;
        public const int Ed50UtmLast = 23038;
        public const int Etrs89UtmFirst = 25828;
        public const int Etrs89UtmLast = 25838;
        public const int Wgs84UtmNorthFirst = 32601;
        public const int Wgs84UtmNorthLast = 32660;
        public const int Wgs84UpsNorth = 32661;
        public const int Wgs84UtmSouthFirst = 32701;
        public const int Wgs84UtmSouthLast = 32760;
        public const int Wgs84UpsSouth = 32761;

        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>();

        public override MapProjection GetProjection(string crsId)
        {
            MapProjection projection = null;
            var str = crsId.StartsWith("EPSG:") ? crsId.Substring(5)
                    : crsId.StartsWith("AUTO2:") ? crsId.Substring(6)
                    : null;

            if (int.TryParse(str, out int code))
            {
                if (CoordinateSystemWkts.TryGetValue(code, out string wkt))
                {
                    projection = new GeoApiProjection(wkt);
                }
                else
                {
                    switch (code)
                    {
                        case WorldMercator:
                            projection = new WorldMercatorProjection();
                            break;

                        case WebMercator:
                            projection = new WebMercatorProjection();
                            break;

                        case AutoUtm:
                            projection = new AutoUtmProjection();
                            break;

                        case int c when c >= Ed50UtmFirst && c <= Ed50UtmLast:
                            projection = new Ed50UtmProjection(code % 100);
                            break;

                        case int c when c >= Etrs89UtmFirst && c <= Etrs89UtmLast:
                            projection = new Etrs89UtmProjection(code % 100);
                            break;

                        case int c when c >= Wgs84UtmNorthFirst && c <= Wgs84UtmNorthLast:
                            projection = new Wgs84UtmProjection(code % 100, true);
                            break;

                        case int c when c >= Wgs84UtmSouthFirst && c <= Wgs84UtmSouthLast:
                            projection = new Wgs84UtmProjection(code % 100, false);
                            break;

                        case Wgs84UpsNorth:
                            projection = new UpsNorthProjection();
                            break;

                        case Wgs84UpsSouth:
                            projection = new UpsSouthProjection();
                            break;

                        default:
                            break;
                    }
                }
            }

            return projection ?? base.GetProjection(crsId);
        }
    }
}
