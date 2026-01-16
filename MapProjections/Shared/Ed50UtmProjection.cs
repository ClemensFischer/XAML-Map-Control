using System;

namespace MapControl.Projections
{
    /// <summary>
    /// ED50 Universal Transverse Mercator Projection.
    /// </summary>
    public class Ed50UtmProjection : ProjNetMapProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 23000 + FirstZone;
        public const int LastZoneEpsgCode = 23000 + LastZone;

        public int Zone { get; }

        public Ed50UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ED50 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt
                = $"PROJCS[\"ED50 / UTM zone {zone}N\","
                + WktConstants.GeogCsEd50 + ","
                + "PROJECTION[\"Transverse_Mercator\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + $"AUTHORITY[\"EPSG\",\"230{zone:00}\"]]";
        }
    }
}
