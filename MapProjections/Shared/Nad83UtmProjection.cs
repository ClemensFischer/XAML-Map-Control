namespace MapControl.Projections
{
    /// <summary>
    /// NAD83 Universal Transverse Mercator Projection - EPSG:26901 to EPSG:26923.
    /// </summary>
    public class Nad83UtmProjection : ProjNetMapProjection
    {
        public int Zone { get; }

        public Nad83UtmProjection(int zone)
            : base(new MapControl.Nad83UtmProjection(zone))
        {
            Zone = zone;
            CoordinateSystemWkt =
                $"PROJCS[\"NAD83 / UTM zone {zone}N\"," +
                WktConstants.GeogCsNad83 + "," +
                "PROJECTION[\"Transverse_Mercator\"]," +
                "PARAMETER[\"latitude_of_origin\",0]," +
                $"PARAMETER[\"central_meridian\",{6 * zone - 183}]," +
                "PARAMETER[\"scale_factor\",0.9996]," +
                "PARAMETER[\"false_easting\",500000]," +
                "PARAMETER[\"false_northing\",0]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AXIS[\"Easting\",EAST]," +
                "AXIS[\"Northing\",NORTH]," +
                $"AUTHORITY[\"EPSG\",\"269{zone:00}\"]]";
        }
    }
}
