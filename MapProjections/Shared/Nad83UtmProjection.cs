using System;

namespace MapControl.Projections
{
    /// <summary>
    /// NAD83 Universal Transverse Mercator Projection.
    /// </summary>
    public class Nad83UtmProjection : ProjNetMapProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 23;
        public const int FirstZoneEpsgCode = 26900 + FirstZone;
        public const int LastZoneEpsgCode = 26900 + LastZone;

        public int Zone { get; }

        public Nad83UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD83 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt
                = $"PROJCS[\"NAD83 / UTM zone {zone}N\","
                + WktConstants.GeoGcsNad83 + ","
                + "PROJECTION[\"Transverse_Mercator\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + $"AUTHORITY[\"EPSG\",\"269{zone:00}\"]]";
        }
    }
}
