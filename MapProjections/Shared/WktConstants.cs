namespace MapControl.Projections
{
    public static class WktConstants
    {
        private const string PrimeMeridian = "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],";
        private const string UnitDegree = "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],";

        public const string SpheroidWgs84 = "SPHEROID[\"WGS 84\",6378137,298.257223563]";
        public const string SpheroidGrs1980 = "SPHEROID[\"GRS 1980\",6378137,298.257222101]";
        public const string SpheroidGrs1967Modified = "SPHEROID[\"GRS 1967 Modified\",6378160,298.25]";
        public const string SpheroidInternational1924 = "SPHEROID[\"International 1924\",6378388,297]";
        public const string SpheroidClarke1866 = "SPHEROID[\"Clarke 1866\",6378206.4,294.978698213898]";
        public const string SpheroidBessel1841 = "SPHEROID[\"Bessel 1841\",6377397.155,299.1528128]";

        public const string GeoGcsWgs84
            = "GEOGCS[\"WGS 84\","
            + "DATUM[\"WGS_1984\","
            + SpheroidWgs84 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4326\"]]";

        public const string GeoGcsEd50
            = "GEOGCS[\"ED50\","
            + "DATUM[\"European_Datum_1950\","
            + SpheroidInternational1924 + ","
            + "TOWGS84[-87,-98,-121,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4230\"]]";

        public const string GeoGcsEtrs89
            = "GEOGCS[\"ETRS89\","
            + "DATUM[\"European_Terrestrial_Reference_System_1989\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4258\"]]";

        public const string GeoGcsGgrs87
            = "GEOGCS[\"GGRS87\","
            + "DATUM[\"Greek_Geodetic_Reference_System_1987\","
            + SpheroidGrs1980 + ","
            + "TOWGS84[-199.87,74.79,246.62,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4121\"]]";

        public const string GeoGcsEtrf2000Pl
            = "GEOGCS[\"ETRF2000-PL\","
            + "DATUM[\"ETRF2000_Poland\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"9702\"]]";

        public const string GeoGcsNad83
            = "GEOGCS[\"NAD83\","
            + "DATUM[\"North_American_Datum_1983\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4269\"]]";

        public const string GeoGcsNad27
            = "GEOGCS[\"NAD27\","
            + "DATUM[\"North_American_Datum_1927\","
            + SpheroidClarke1866 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4267\"]]";

        public const string GeoGcsSad69
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGrs1967Modified + ","
            + "TOWGS84[-57,1,-41,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        public const string GeoGcsSad69_96
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969_96\","
            + SpheroidGrs1967Modified + ","
            + "TOWGS84[-67.35,3.88,-38.22,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"5527\"]]";

        public const string GeoGcsCh1903
            = "GEOGCS[\"CH1903\","
            + "DATUM[\"CH1903\","
            + SpheroidBessel1841 + ","
            + "TOWGS84[674.374,15.056,405.346,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4149\"]]";
    }
}
