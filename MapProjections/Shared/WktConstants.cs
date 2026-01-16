namespace MapControl.Projections
{
    /// <summary>
    /// Well-known text representations of geographic and projected coordinate systems
    /// taken from epsg.io.
    /// </summary>
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

        public const string GeogCsWgs84
            = "GEOGCS[\"WGS 84\","
            + "DATUM[\"WGS_1984\","
            + SpheroidWgs84 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4326\"]]";

        public const string GeogCsEd50
            = "GEOGCS[\"ED50\","
            + "DATUM[\"European_Datum_1950\","
            + SpheroidInternational1924 + ","
            + "TOWGS84[-87,-98,-121,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4230\"]]";

        public const string GeogCsEtrs89
            = "GEOGCS[\"ETRS89\","
            + "DATUM[\"European_Terrestrial_Reference_System_1989\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4258\"]]";

        public const string GeogCsGgrs87
            = "GEOGCS[\"GGRS87\","
            + "DATUM[\"Greek_Geodetic_Reference_System_1987\","
            + SpheroidGrs1980 + ","
            + "TOWGS84[-199.87,74.79,246.62,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4121\"]]";

        public const string GeogCsEtrf2000Pl
            = "GEOGCS[\"ETRF2000-PL\","
            + "DATUM[\"ETRF2000_Poland\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"9702\"]]";

        public const string GeogCsNad83
            = "GEOGCS[\"NAD83\","
            + "DATUM[\"North_American_Datum_1983\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4269\"]]";

        public const string GeogCsNad27
            = "GEOGCS[\"NAD27\","
            + "DATUM[\"North_American_Datum_1927\","
            + SpheroidClarke1866 + "],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4267\"]]";

        public const string GeogCsSad69
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGrs1967Modified + ","
            + "TOWGS84[-57,1,-41,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        public const string GeogCsSad69_96
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969_96\","
            + SpheroidGrs1967Modified + ","
            + "TOWGS84[-67.35,3.88,-38.22,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"5527\"]]";

        public const string GeogCsCh1903
            = "GEOGCS[\"CH1903\","
            + "DATUM[\"CH1903\","
            + SpheroidBessel1841 + ","
            + "TOWGS84[674.374,15.056,405.346,0,0,0,0]],"
            + PrimeMeridian
            + UnitDegree
            + "AUTHORITY[\"EPSG\",\"4149\"]]";

        public const string ProjCsGgrs87
            = "PROJCS[\"GGRS87 / Greek Grid\","
            + GeogCsGgrs87 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",24],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",0],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"2100\"]]";

        public const string ProjCsEtrf2000Pl
            = "PROJCS[\"ETRF2000-PL / CS92\","
            + GeogCsEtrf2000Pl + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",19],"
            + "PARAMETER[\"scale_factor\",0.9993],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",-5300000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AUTHORITY[\"EPSG\",\"2180\"]]";

        public const string ProjCsEtrs89Utm32NzEN
            = "PROJCS[\"ETRS89 / UTM zone 32N (zE-N)\","
            + GeogCsEtrs89 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",9],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",32500000],"
            + "PARAMETER[\"false_northing\",0],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"4647\"]]";

        public const string ProjCsEtrs89LccEurope
            = "PROJCS[\"ETRS89-extended / LCC Europe\","
            + GeogCsEtrs89 + ","
            + "PROJECTION[\"Lambert_Conformal_Conic_2SP\"],"
            + "PARAMETER[\"latitude_of_origin\",52],"
            + "PARAMETER[\"central_meridian\",10],"
            + "PARAMETER[\"standard_parallel_1\",35],"
            + "PARAMETER[\"standard_parallel_2\",65],"
            + "PARAMETER[\"false_easting\",4000000],"
            + "PARAMETER[\"false_northing\",2800000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AUTHORITY[\"EPSG\",\"3034\"]]";

        public const string ProjCsEtrs89LaeaEurope
            = "PROJCS[\"ETRS89-extended / LAEA Europe\","
            + GeogCsEtrs89 + ","
            + "PROJECTION[\"Lambert_Azimuthal_Equal_Area\"],"
            + "PARAMETER[\"latitude_of_center\",52],"
            + "PARAMETER[\"longitude_of_center\",10],"
            + "PARAMETER[\"false_easting\",4321000],"
            + "PARAMETER[\"false_northing\",3210000]"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AUTHORITY[\"EPSG\",\"3035\"]]";

        public const string ProjCsEtrs89LccGermanyNE
            = "PROJCS[\"ETRS89 / LCC Germany (N-E)\","
            + GeogCsEtrs89 + ","
            + "PROJECTION[\"Lambert_Conformal_Conic_2SP\"],"
            + "PARAMETER[\"latitude_of_origin\",51],"
            + "PARAMETER[\"central_meridian\",10.5],"
            + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
            + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
            + "PARAMETER[\"false_easting\",0],"
            + "PARAMETER[\"false_northing\",0],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AUTHORITY[\"EPSG\",\"4839\"]]";

        public const string ProjCsEtrs89LccGermanyEN
            = "PROJCS[\"ETRS89 / LCC Germany (E-N)\","
            + GeogCsEtrs89 + ","
            + "PROJECTION[\"Lambert_Conformal_Conic_2SP\"],"
            + "PARAMETER[\"latitude_of_origin\",51],"
            + "PARAMETER[\"central_meridian\",10.5],"
            + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
            + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
            + "PARAMETER[\"false_easting\",0],"
            + "PARAMETER[\"false_northing\",0],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"5243\"]]";

        public const string ProjCsCh1903Lv95
            = "PROJCS[\"CH1903 / LV95\","
            + GeogCsCh1903 + ","
            + "PROJECTION[\"Hotine_Oblique_Mercator_Azimuth_Center\"],"
            + "PARAMETER[\"latitude_of_center\",46.9524055555556],"
            + "PARAMETER[\"longitude_of_center\",7.43958333333333],"
            + "PARAMETER[\"azimuth\",90],"
            + "PARAMETER[\"rectified_grid_angle\",90],"
            + "PARAMETER[\"scale_factor\",1],"
            + "PARAMETER[\"false_easting\",2600000],"
            + "PARAMETER[\"false_northing\",1200000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"2056\"]]";

        public const string ProjCsCh1903Lv03
            = "PROJCS[\"CH1903 / LV03\","
            + GeogCsCh1903 + ","
            + "PROJECTION[\"Hotine_Oblique_Mercator_Azimuth_Center\"],"
            + "PARAMETER[\"latitude_of_center\",46.9524055555556],"
            + "PARAMETER[\"longitude_of_center\",7.43958333333333],"
            + "PARAMETER[\"azimuth\",90],"
            + "PARAMETER[\"rectified_grid_angle\",90],"
            + "PARAMETER[\"scale_factor\",1],"
            + "PARAMETER[\"false_easting\",600000],"
            + "PARAMETER[\"false_northing\",200000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"21781\"]]";

        public const string ProjCsSad69Utm17S
            = "PROJCS[\"SAD69 / UTM zone 17S\","
            + GeogCsSad69 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-81],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29187\"]]";

        public const string ProjCsSad69Utm18S
            = "PROJCS[\"SAD69 / UTM zone 18S\","
            + GeogCsSad69 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-75],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29188\"]]";

        public const string ProjCsSad69Utm19S
            = "PROJCS[\"SAD69 / UTM zone 19S\","
            + GeogCsSad69 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-69],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29189\"]]";

        public const string ProjCsSad69Utm20S
            = "PROJCS[\"SAD69 / UTM zone 20S\","
            + GeogCsSad69 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-63],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29190\"]]";

        public const string ProjCsSad69Utm21S
            = "PROJCS[\"SAD69 / UTM zone 21S\","
            + GeogCsSad69 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-57],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29191\"]]";

        public const string ProjCsSad69Utm22S
            = "PROJCS[\"SAD69 / UTM zone 22S\","
            + GeogCsSad69_96 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-51],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29192\"]]";

        public const string ProjCsSad69Utm23S
            = "PROJCS[\"SAD69 / UTM zone 23S\","
            + GeogCsSad69_96 + ","
            + "PROJECTION[\"Transverse_Mercator\"],"
            + "PARAMETER[\"latitude_of_origin\",0],"
            + "PARAMETER[\"central_meridian\",-45],"
            + "PARAMETER[\"scale_factor\",0.9996],"
            + "PARAMETER[\"false_easting\",500000],"
            + "PARAMETER[\"false_northing\",10000000],"
            + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
            + "AXIS[\"Easting\",EAST],"
            + "AXIS[\"Northing\",NORTH],"
            + "AUTHORITY[\"EPSG\",\"29193\"]]";
    }
}
