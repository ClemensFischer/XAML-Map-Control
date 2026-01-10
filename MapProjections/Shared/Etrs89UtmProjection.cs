using System;

namespace MapControl.Projections
{
    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection.
    /// </summary>
    public class Etrs89UtmProjection : GeoApiProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        public int Zone { get; }

        public Etrs89UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ETRS89 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt
                = $"PROJCS[\"ETRS89 / UTM zone {zone}N\","
                + "GEOGCS[\"ETRS89\","
                + "DATUM[\"European_Terrestrial_Reference_System_1989\","
                + "SPHEROID[\"GRS 1980\",6378137,298.257222101]],"
                + "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],"
                + "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],"
                + "AUTHORITY[\"EPSG\",\"4258\"]],"
                + "PROJECTION[\"Transverse_Mercator\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + $"AUTHORITY[\"EPSG\",\"258{zone:00}\"]]";
        }
    }
}
