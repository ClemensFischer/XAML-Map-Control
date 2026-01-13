using System.Collections.Generic;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        internal const string SpheroidGRS1980 = "SPHEROID[\"GRS 1980\",6378137,298.257222101]";
        internal const string SpheroidGRS1967Modified = "SPHEROID[\"GRS 1967 Modified\",6378160,298.25]";
        internal const string PrimeMeridian = "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]]";
        internal const string UnitDegree = "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]]";
        internal const string UnitMeter = "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]";
        internal const string ProjectionTM = "PROJECTION[\"Transverse_Mercator\"]";
        internal const string ProjectionLCC = "PROJECTION[\"Lambert_Conformal_Conic_2SP\"]";
        internal const string AxisEast = "AXIS[\"Easting\",EAST]";
        internal const string AxisNorth = "AXIS[\"Northing\",NORTH]";

        internal const string GeoGcsETRS89
            = "GEOGCS[\"ETRS89\","
            + "DATUM[\"European_Terrestrial_Reference_System_1989\","
            + SpheroidGRS1980 + "],"
            + PrimeMeridian + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4258\"]]";

        internal const string GeoGcsGGRS87
            = "GEOGCS[\"GGRS87\","
            + "DATUM[\"Greek_Geodetic_Reference_System_1987\","
            + SpheroidGRS1980 + ",TOWGS84[-199.87,74.79,246.62,0,0,0,0]],"
            + PrimeMeridian + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4121\"]]";

        internal const string GeoGcsETRF2000PL
            = "GEOGCS[\"ETRF2000-PL\","
            + "DATUM[\"ETRF2000_Poland\","
            + SpheroidGRS1980 + "],"
            + PrimeMeridian + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"9702\"]]";

        internal const string GeoGcsSAD69A
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGRS1967Modified + ",TOWGS84[-57,1,-41,0,0,0,0]],"
            + PrimeMeridian + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        internal const string GeoGcsSAD69B
            = "GEOGCS[\"SAD69\","
            + "DATUM[\"South_American_Datum_1969\","
            + SpheroidGRS1967Modified + ",TOWGS84[-67.35,3.88,-38.22,0,0,0,0]],"
            + PrimeMeridian + ","
            + UnitDegree + ","
            + "AUTHORITY[\"EPSG\",\"4618\"]]";

        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>
        {
            {
                2100, "PROJCS[\"GGRS87 / Greek Grid\","
                    + GeoGcsGGRS87 + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",24],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"2100\"]]"
            },
            {
                2180, "PROJCS[\"ETRF2000-PL / CS92\","
                    + GeoGcsETRF2000PL + ","
                    + ProjectionTM + ","
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
                    + GeoGcsETRS89 + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",9],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",32500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"4647\"]]"
            },
            {
                29187, "PROJCS[\"SAD69 / UTM zone 17S\","
                    + GeoGcsSAD69A + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-81],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29187\"]]"
            },
            {
                29188, "PROJCS[\"SAD69 / UTM zone 18S\","
                    + GeoGcsSAD69A + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-75],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29188\"]]"
            },
            {
                29189, "PROJCS[\"SAD69 / UTM zone 19S\","
                    + GeoGcsSAD69A + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-69],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29189\"]]"
            },
            {
                29190, "PROJCS[\"SAD69 / UTM zone 20S\","
                    + GeoGcsSAD69A + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-63],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29190\"]]"
            },
            {
                29191, "PROJCS[\"SAD69 / UTM zone 21S\","
                    + GeoGcsSAD69A + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-57],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29191\"]]"
            },
            {
                29192, "PROJCS[\"SAD69 / UTM zone 22S\","
                    + GeoGcsSAD69B + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-51],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29192\"]]"
            },
            {
                29193, "PROJCS[\"SAD69 / UTM zone 23S\","
                    + GeoGcsSAD69B + ","
                    + ProjectionTM + ","
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-45],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"29193\"]]"
            },
            {
                3034, "PROJCS[\"ETRS89-extended / LCC Europe\","
                    + GeoGcsETRS89 + ","
                    + ProjectionLCC + ","
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
                    + GeoGcsETRS89 + ","
                    + ProjectionLCC + ","
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
                    + GeoGcsETRS89 + ","
                    + ProjectionLCC + ","
                    + "PARAMETER[\"latitude_of_origin\",51],"
                    + "PARAMETER[\"central_meridian\",10.5],"
                    + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
                    + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
                    + "PARAMETER[\"false_easting\",0],"
                    + "PARAMETER[\"false_northing\",0],"
                    + UnitMeter + ","
                    + AxisEast + ","
                    + AxisNorth + ","
                    + "AUTHORITY[\"EPSG\",\"5243\"]]"
            }
        };

        public override MapProjection GetProjection(string crsId) => crsId switch
        {
            MapControl.WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
            MapControl.WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
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
            _ => CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt) ? new GeoApiProjection(wkt) : base.GetProjection(epsgCode)
        };
    }
}
