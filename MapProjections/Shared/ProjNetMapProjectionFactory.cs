using System.Collections.Generic;

namespace MapControl.Projections
{
    public class ProjNetMapProjectionFactory : MapProjectionFactory
    {
        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>
        {
            {
                2100, "PROJCS[\"GGRS87 / Greek Grid\","
                    + WktConstants.GeoGcsGgrs87 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",24],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"2100\"]]"
            },
            {
                2180, "PROJCS[\"ETRF2000-PL / CS92\","
                    + WktConstants.GeoGcsEtrf2000Pl + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",19],"
                    + "PARAMETER[\"scale_factor\",0.9993],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",-5300000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AUTHORITY[\"EPSG\",\"2180\"]]"
            },
            {
                4647, "PROJCS[\"ETRS89 / UTM zone 32N (zE-N)\","
                    + WktConstants.GeoGcsEtrs89 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",9],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",32500000],"
                    + "PARAMETER[\"false_northing\",0],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"4647\"]]"
            },
            {
                29187, "PROJCS[\"SAD69 / UTM zone 17S\","
                    + WktConstants.GeoGcsSad69 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-81],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29187\"]]"
            },
            {
                29188, "PROJCS[\"SAD69 / UTM zone 18S\","
                    + WktConstants.GeoGcsSad69 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-75],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29188\"]]"
            },
            {
                29189, "PROJCS[\"SAD69 / UTM zone 19S\","
                    + WktConstants.GeoGcsSad69 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-69],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29189\"]]"
            },
            {
                29190, "PROJCS[\"SAD69 / UTM zone 20S\","
                    + WktConstants.GeoGcsSad69 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-63],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29190\"]]"
            },
            {
                29191, "PROJCS[\"SAD69 / UTM zone 21S\","
                    + WktConstants.GeoGcsSad69 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-57],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29191\"]]"
            },
            {
                29192, "PROJCS[\"SAD69 / UTM zone 22S\","
                    + WktConstants.GeoGcsSad69_96 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-51],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29192\"]]"
            },
            {
                29193, "PROJCS[\"SAD69 / UTM zone 23S\","
                    + WktConstants.GeoGcsSad69_96 + ","
                    + "PROJECTION[\"Transverse_Mercator\"],"
                    + "PARAMETER[\"latitude_of_origin\",0],"
                    + "PARAMETER[\"central_meridian\",-45],"
                    + "PARAMETER[\"scale_factor\",0.9996],"
                    + "PARAMETER[\"false_easting\",500000],"
                    + "PARAMETER[\"false_northing\",10000000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AXIS[\"Easting\",EAST],"
                    + "AXIS[\"Northing\",NORTH],"
                    + "AUTHORITY[\"EPSG\",\"29193\"]]"
            },
            {
                3034, "PROJCS[\"ETRS89-extended / LCC Europe\","
                    + WktConstants.GeoGcsEtrs89 + ","
                    + "PROJECTION[\"Lambert_Conformal_Conic_2SP\"],"
                    + "PARAMETER[\"latitude_of_origin\",52],"
                    + "PARAMETER[\"central_meridian\",10],"
                    + "PARAMETER[\"standard_parallel_1\",35],"
                    + "PARAMETER[\"standard_parallel_2\",65],"
                    + "PARAMETER[\"false_easting\",4000000],"
                    + "PARAMETER[\"false_northing\",2800000],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AUTHORITY[\"EPSG\",\"3034\"]]"
            },
            {
                3035, "PROJCS[\"ETRS89-extended / LAEA Europe\","
                    + WktConstants.GeoGcsEtrs89 + ","
                    + "PROJECTION[\"Lambert_Azimuthal_Equal_Area\"],"
                    + "PARAMETER[\"latitude_of_center\",52],"
                    + "PARAMETER[\"longitude_of_center\",10],"
                    + "PARAMETER[\"false_easting\",4321000],"
                    + "PARAMETER[\"false_northing\",3210000]"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AUTHORITY[\"EPSG\",\"3035\"]]"
            },
            {
                4839, "PROJCS[\"ETRS89 / LCC Germany (N-E)\","
                    + WktConstants.GeoGcsEtrs89 + ","
                    + "PROJECTION[\"Lambert_Conformal_Conic_2SP\"],"
                    + "PARAMETER[\"latitude_of_origin\",51],"
                    + "PARAMETER[\"central_meridian\",10.5],"
                    + "PARAMETER[\"standard_parallel_1\",48.6666666666667],"
                    + "PARAMETER[\"standard_parallel_2\",53.6666666666667],"
                    + "PARAMETER[\"false_easting\",0],"
                    + "PARAMETER[\"false_northing\",0],"
                    + "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],"
                    + "AUTHORITY[\"EPSG\",\"4839\"]]"
            },
            {
                5243, "PROJCS[\"ETRS89 / LCC Germany (E-N)\","
                    + WktConstants.GeoGcsEtrs89 + ","
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
                    + "AUTHORITY[\"EPSG\",\"5243\"]]"
            },
            {
                21781, "PROJCS[\"CH1903 / LV03\","
                    + WktConstants.GeoGcsCh1903 + ","
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
                    + "AUTHORITY[\"EPSG\",\"21781\"]]"
            },
            {
                2056, "PROJCS[\"CH1903 / LV95\","
                    + WktConstants.GeoGcsCh1903 + ","
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
                    + "AUTHORITY[\"EPSG\",\"2056\"]]"
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
