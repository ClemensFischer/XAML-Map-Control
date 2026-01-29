namespace MapControl.Projections
{
    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection - EPSG:25828 to EPSG:25838.
    /// </summary>
    public class Etrs89UtmProjection : ProjNetMapProjection
    {
        public int Zone { get; }

        public Etrs89UtmProjection(int zone)
            : base(new MapControl.Etrs89UtmProjection(zone))
        {
            Zone = zone;
            CoordinateSystemWkt =
                $"PROJCS[\"ETRS89 / UTM zone {zone}N\"," +
                WktConstants.GeogCsEtrs89 + "," +
                "PROJECTION[\"Transverse_Mercator\"]," +
                "PARAMETER[\"latitude_of_origin\",0]," +
                $"PARAMETER[\"central_meridian\",{6 * zone - 183}]," +
                "PARAMETER[\"scale_factor\",0.9996]," +
                "PARAMETER[\"false_easting\",500000]," +
                "PARAMETER[\"false_northing\",0]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AXIS[\"Easting\",EAST]," +
                "AXIS[\"Northing\",NORTH]," +
                $"AUTHORITY[\"EPSG\",\"258{zone:00}\"]]";
        }
    }
}
