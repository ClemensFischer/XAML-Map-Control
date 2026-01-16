using System;

namespace MapControl.Projections
{
    /// <summary>
    /// NAD27 Universal Transverse Mercator Projection.
    /// </summary>
    public class Nad27UtmProjection : ProjNetMapProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 22;
        public const int FirstZoneEpsgCode = 26700 + FirstZone;
        public const int LastZoneEpsgCode = 26700 + LastZone;

        public int Zone { get; }

        public Nad27UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD27 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt =
                $"PROJCS[\"NAD27 / UTM zone {zone}N\"," +
                WktConstants.GeogCsNad27 + "," +
                "PROJECTION[\"Transverse_Mercator\"]," +
                "PARAMETER[\"latitude_of_origin\",0]," +
                $"PARAMETER[\"central_meridian\",{6 * zone - 183}]," +
                "PARAMETER[\"scale_factor\",0.9996]," +
                "PARAMETER[\"false_easting\",500000]," +
                "PARAMETER[\"false_northing\",0]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AXIS[\"Easting\",EAST]," +
                "AXIS[\"Northing\",NORTH]," +
                $"AUTHORITY[\"EPSG\",\"267{zone:00}\"]]";
        }
    }
}
