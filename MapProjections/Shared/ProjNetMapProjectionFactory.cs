using System.Collections.Generic;

namespace MapControl.Projections
{
    public class ProjNetMapProjectionFactory : MapProjectionFactory
    {
        internal const string SpheroidWgs84 = "SPHEROID[\"WGS 84\",6378137,298.257223563]";
        internal const string SpheroidGrs1980 = "SPHEROID[\"GRS 1980\",6378137,298.257222101]";
        internal const string SpheroidGrs1967Modified = "SPHEROID[\"GRS 1967 Modified\",6378160,298.25]";
        internal const string PrimeMeridianGreenwich = "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]]";
        internal const string UnitDegree = "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]]";
        internal const string UnitMeter = "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]";
        internal const string ProjectionTransverseMercator = "PROJECTION[\"Transverse_Mercator\"]";
        internal const string ProjectionLambertConformalConic = "PROJECTION[\"Lambert_Conformal_Conic_2SP\"]";
        internal const string AxisEasting = "AXIS[\"Easting\",EAST]";
        internal const string AxisNorthing = "AXIS[\"Northing\",NORTH]";

        internal const string GeoGcsWgs84
            = "GEOGCS[\"WGS 84\","
            + "DATUM[\"WGS_1984\","
            + SpheroidWgs84 + "],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4326\"]]";

        internal const string GeoGcsEtrs89
            = "GEOGCS[\"ETRS89\","
            + "DATUM[\"European_Terrestrial_Reference_System_1989\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4258\"]]";

        internal const string GeoGcsGgrs87
            = "GEOGCS[\"GGRS87\","
            + "DATUM[\"Greek_Geodetic_Reference_System_1987\","
            + SpheroidGrs1980 + ",TOWGS84[-199.87,74.79,246.62,0,0,0,0]],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4121\"]]";

        internal const string GeoGcsEtrf2000Pl
            = "GEOGCS[\"ETRF2000-PL\","
            + "DATUM[\"ETRF2000_Poland\","
            + SpheroidGrs1980 + "],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"9702\"]]";

        internal const string GeoGcsSad69A
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGrs1967Modified + ",TOWGS84[-57,1,-41,0,0,0,0]],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        internal const string GeoGcsSad69B
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGrs1967Modified + ",TOWGS84[-67.35,3.88,-38.22,0,0,0,0]],"
            + PrimeMeridianGreenwich + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>
        {
            {
                2100, "PROJCS[\"GGRS87 / Greek Grid\","
                    + GeoGcsGgrs87 + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",24],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"2100\"]]"
            },
            {
                2180, "PROJCS[\"ETRF2000-PL / CS92\","
                    + GeoGcsEtrf2000Pl + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",19],"
                    + "PARAMETER[\"scale_factor\",0.9993],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",-5300000],"
                    + UnitMeter + ","
                    + "AUTHORITY[\"EPSG\",\"2180\"]]"
            },
            {
                4647, "PROJCS[\"ETRS89 / UTM zone 32N (zE-N)\","
                    + GeoGcsEtrs89 + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",9],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",32500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"4647\"]]"
            },
            {
                29187, "PROJCS[\"SAD69 / UTM zone 17S\","
                    + GeoGcsSad69A + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-81],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29187\"]]"
            },
            {
                29188, "PROJCS[\"SAD69 / UTM zone 18S\","
                    + GeoGcsSad69A + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-75],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29188\"]]"
            },
            {
                29189, "PROJCS[\"SAD69 / UTM zone 19S\","
                    + GeoGcsSad69A + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-69],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29189\"]]"
            },
            {
                29190, "PROJCS[\"SAD69 / UTM zone 20S\","
                    + GeoGcsSad69A + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-63],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29190\"]]"
            },
            {
                29191, "PROJCS[\"SAD69 / UTM zone 21S\","
                    + GeoGcsSad69A + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-57],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29191\"]]"
            },
            {
                29192, "PROJCS[\"SAD69 / UTM zone 22S\","
                    + GeoGcsSad69B + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-51],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29192\"]]"
            },
            {
                29193, "PROJCS[\"SAD69 / UTM zone 23S\","
                    + GeoGcsSad69B + ","
                    + ProjectionTransverseMercator + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-45],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"29193\"]]"
            },
            {
                3034, "PROJCS[\"ETRS89-extended / LCC Europe\","
                    + GeoGcsEtrs89 + ","
                    + ProjectionLambertConformalConic + ","
                    + "PARAMETER[\"latitude_of_origin\",52],"
                    + "PARAMETER[\"central_meridian\",10],"
                    + "PARAMETER[\"standard_parallel_1\",35],"
                    + "PARAMETER[\"standard_parallel_2\",65],"
                    + "PARAMETER[\"false_easting\",4000000],"
                    + "PARAMETER[\"false_northing\",2800000],"
                    + UnitMeter + ","
                    + "AUTHORITY[\"EPSG\",\"3034\"]]"
            },
            {
                4839, "PROJCS[\"ETRS89 / LCC Germany (N-E)\","
                    + GeoGcsEtrs89 + ","
                    + ProjectionLambertConformalConic + ","
                    + "PARAMETER[\"latitude_of_origin\",51],"
                    + "PARAMETER[\"central_meridian\",10.5],"
                    + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
                    + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
                    + "PARAMETER[\"false_easting\",0],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + "AUTHORITY[\"EPSG\",\"4839\"]]"
            },
            {
                5243, "PROJCS[\"ETRS89 / LCC Germany (E-N)\","
                    + GeoGcsEtrs89 + ","
                    + ProjectionLambertConformalConic + ","
                    + "PARAMETER[\"latitude_of_origin\",51],"
                    + "PARAMETER[\"central_meridian\",10.5],"
                    + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
                    + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
                    + "PARAMETER[\"false_easting\",0],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEasting + ","
                    + AxisNorthing + ","
                    + "AUTHORITY[\"EPSG\",\"5243\"]]"
            }
        };

        public override MapProjection GetProjection(string crsId) => crsId switch
        {
            MapControl.WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
            MapControl.WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
            MapControl.Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
            MapControl.Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
            MapControl.Wgs84AutoUtmProjection.DefaultCrsId => new Wgs84AutoUtmProjection(),
            _ => base.GetProjection(crsId)
        };

        public override MapProjection GetProjection(int epsgCode) => epsgCode switch
        {
            var code when code >= Ed50UtmProjection.FirstZoneEpsgCode && code <= Ed50UtmProjection.LastZoneEpsgCode => new Ed50UtmProjection(epsgCode % 100),
            var code when code >= Etrs89UtmProjection.FirstZoneEpsgCode && code <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(epsgCode % 100),
            var code when code >= Nad27UtmProjection.FirstZoneEpsgCode && code <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(epsgCode % 100),
            var code when code >= Nad83UtmProjection.FirstZoneEpsgCode && code <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(epsgCode % 100),
            var code when code >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && code <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.North),
            var code when code >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && code <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.South),
            _ => CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt) ? new ProjNetMapProjection(wkt) : base.GetProjection(epsgCode)
        };
    }
}
