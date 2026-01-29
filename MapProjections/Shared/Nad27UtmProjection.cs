namespace MapControl.Projections
{
    /// <summary>
    /// NAD27 Universal Transverse Mercator Projection - EPSG:26701 to EPSG:26722.
    /// </summary>
    public class Nad27UtmProjection : ProjNetMapProjection
    {
        public int Zone { get; }

        public Nad27UtmProjection(int zone)
            : base(new MapControl.Nad27UtmProjection(zone))
        {
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
