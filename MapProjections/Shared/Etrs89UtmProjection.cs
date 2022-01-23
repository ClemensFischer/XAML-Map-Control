// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl.Projections
{
    public class Etrs89UtmProjection : GeoApiProjection
    {
        public Etrs89UtmProjection(int zone)
        {
            SetZone(zone);
        }

        protected void SetZone(int zone)
        {
            if (zone < 28 || zone > 38)
            {
                throw new ArgumentException($"Invalid UTM zone {zone}.", nameof(zone));
            }

            const string wktFormat
                = "PROJCS[\"ETRS89 / UTM zone {0}N\","
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
                + "PARAMETER[\"central_meridian\",{1}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,"
                + "AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + "AUTHORITY[\"EPSG\",\"258{0}\"]]";

            WKT = string.Format(wktFormat, zone, 6 * zone - 183);
        }
    }
}
