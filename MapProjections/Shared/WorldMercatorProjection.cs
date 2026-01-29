namespace MapControl.Projections
{
    /// <summary>
    /// Elliptical Mercator Projection implemented by setting the WKT property of a ProjNetMapProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44-45.
    /// </summary>
    public class WorldMercatorProjection : ProjNetMapProjection
    {
        public WorldMercatorProjection()
            : base(new MapControl.WorldMercatorProjection())
        {
            CoordinateSystemWkt =
                "PROJCS[\"WGS 84 / World Mercator\"," +
                WktConstants.GeogCsWgs84 + "," +
                "PROJECTION[\"Mercator_1SP\"]," +
                "PARAMETER[\"latitude_of_origin\",0]," +
                "PARAMETER[\"central_meridian\",0]," +
                "PARAMETER[\"scale_factor\",1]," +
                "PARAMETER[\"false_easting\",0]," +
                "PARAMETER[\"false_northing\",0]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AXIS[\"Easting\",EAST]," +
                "AXIS[\"Northing\",NORTH]," +
                "AUTHORITY[\"EPSG\",\"3395\"]]";
        }
    }
}
