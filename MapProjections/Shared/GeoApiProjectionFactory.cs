// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        public const int WorldMercator = 3395;
        public const int WebMercator = 3857;
        public const int Etrs89UtmNorthFirst = 25828;
        public const int Etrs89UtmNorthLast = 25838;
        public const int Wgs84UtmNorthFirst = 32601;
        public const int Wgs84UtmNorthLast = 32660;
        public const int Wgs84UpsNorth = 32661;
        public const int Wgs84UtmSouthFirst = 32701;
        public const int Wgs84UtmSouthLast = 32760;
        public const int Wgs84UpsSouth = 32761;

        public override MapProjection GetProjection(string crsId)
        {
            MapProjection projection = null;

            if (crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode))
            {
                switch (epsgCode)
                {
                    case WorldMercator:
                        projection = new WorldMercatorProjection();
                        break;

                    case WebMercator:
                        projection = new WebMercatorProjection();
                        break;

                    case int c when c >= Etrs89UtmNorthFirst && c <= Etrs89UtmNorthLast:
                        projection = new GeoApiProjection(GetEtrs89UtmWkt(epsgCode));
                        break;

                    case int c when c >= Wgs84UtmNorthFirst && c <= Wgs84UtmNorthLast:
                        projection = new UtmProjection(epsgCode - Wgs84UtmNorthFirst + 1, true);
                        break;

                    case int c when c >= Wgs84UtmSouthFirst && c <= Wgs84UtmSouthLast:
                        projection = new UtmProjection(epsgCode - Wgs84UtmSouthFirst + 1, false);
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

            return projection ?? base.GetProjection(crsId);
        }

        private static string GetEtrs89UtmWkt(int epsgCode)
        {
            const string wktFormat
                = "PROJCS[\"ETRS89 / UTM zone {1}N\","
                + "GEOGCS[\"ETRS89\","
                + "DATUM[\"European_Terrestrial_Reference_System_1989\","
                + "SPHEROID[\"GRS 1980\",6378137,298.257222101,"
                + "AUTHORITY[\"EPSG\",\"7019\"]],"
                + "TOWGS84[0,0,0,0,0,0,0],"
                + "AUTHORITY[\"EPSG\",\"6258\"]],"
                + "PRIMEM[\"Greenwich\",0,"
                + "AUTHORITY[\"EPSG\",\"8901\"]],"
                + "UNIT[\"degree\",0.0174532925199433,"
                + "AUTHORITY[\"EPSG\",\"9122\"]],"
                + "AUTHORITY[\"EPSG\",\"4258\"]],"
                + "PROJECTION[\"Transverse_Mercator\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + "PARAMETER[\"central_meridian\",{2}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,"
                + "AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + "AUTHORITY[\"EPSG\",\"{0}\"]]";

            var zone = epsgCode % 100;
            var centralMeridian = 6 * (epsgCode - Etrs89UtmNorthFirst) - 15;

            return string.Format(wktFormat, epsgCode, zone, centralMeridian);
        }
    }
}
