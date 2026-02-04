using ProjNet.CoordinateSystems;
using System.Collections.Generic;

namespace MapControl.Projections
{
    public class ProjNetMapProjectionFactory : MapProjectionFactory
    {
        public Dictionary<int, string> CoordinateSystemWkts { get; } = new Dictionary<int, string>
        {
            { 2056, WktConstants.ProjCsCh1903Lv95 },
            { 2100, WktConstants.ProjCsGgrs87 },
            { 2180, WktConstants.ProjCsEtrf2000Pl },
            { 3034, WktConstants.ProjCsEtrs89LccEurope },
            { 3035, WktConstants.ProjCsEtrs89LaeaEurope },
            { 4647, WktConstants.ProjCsEtrs89Utm32NzEN },
            { 4839, WktConstants.ProjCsEtrs89LccGermanyNE },
            { 5243, WktConstants.ProjCsEtrs89LccGermanyEN },
            { 21781, WktConstants.ProjCsCh1903Lv03 },
            { 29187, WktConstants.ProjCsSad69Utm17S },
            { 29188, WktConstants.ProjCsSad69Utm18S },
            { 29189, WktConstants.ProjCsSad69Utm19S },
            { 29190, WktConstants.ProjCsSad69Utm20S },
            { 29191, WktConstants.ProjCsSad69Utm21S },
            { 29192, WktConstants.ProjCsSad69Utm22S },
            { 29193, WktConstants.ProjCsSad69Utm23S },
        };

        protected override MapProjection CreateProjection(string crsId)
        {
            return crsId switch
            {
                WebMercatorProjection.DefaultCrsId => new WebMercatorProjection(),
                WorldMercatorProjection.DefaultCrsId => new WorldMercatorProjection(),
                Wgs84UpsNorthProjection.DefaultCrsId => new Wgs84UpsNorthProjection(),
                Wgs84UpsSouthProjection.DefaultCrsId => new Wgs84UpsSouthProjection(),
                _ => base.CreateProjection(crsId)
            };
        }

        protected override MapProjection CreateProjection(int epsgCode)
        {
            if (CoordinateSystemWkts.TryGetValue(epsgCode, out string wkt))
            {
                return new ProjNetMapProjection(wkt);
            }

            return epsgCode switch
            {
                var c when c is >= Ed50UtmProjection.FirstZoneEpsgCode
                            and <= Ed50UtmProjection.LastZoneEpsgCode => new Ed50UtmProjection(c % 100),
                var c when c is >= Etrs89UtmProjection.FirstZoneEpsgCode
                            and <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(c % 100),
                var c when c is >= Nad27UtmProjection.FirstZoneEpsgCode
                            and <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(c % 100),
                var c when c is >= Nad83UtmProjection.FirstZoneEpsgCode
                            and <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(c % 100),
                var c when c is >= Wgs84UtmProjection.FirstZoneNorthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(c % 100, true),
                var c when c is >= Wgs84UtmProjection.FirstZoneSouthEpsgCode
                            and <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(c % 100, false),
                _ => base.CreateProjection(epsgCode)
            };
        }
    }
    /// <summary>
    /// Spherical Mercator Projection - EPSG:3857,
    /// implemented by setting the CoordinateSystem property of a ProjNetMapProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection() : ProjNetMapProjection(ProjectedCoordinateSystem.WebMercator)
    {
        public const string DefaultCrsId = "EPSG:3857";
    }

    /// <summary>
    /// Elliptical Mercator Projection - EPSG:3395,
    /// implemented by setting the CoordinateSystemWkt property of a ProjNetMapProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44-45.
    /// </summary>
    public class WorldMercatorProjection() : ProjNetMapProjection(
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
        "AUTHORITY[\"EPSG\",\"3395\"]]")
    {
        public const string DefaultCrsId = "EPSG:3395";
    }

    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection -
    /// EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760.
    /// </summary>
    public class Wgs84UtmProjection(int zone, bool north) : ProjNetMapProjection(
        ProjectedCoordinateSystem.WGS84_UTM(zone, north))
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        public int Zone => zone;
    }

    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection - EPSG:25828 to EPSG:25838.
    /// </summary>
    public class Etrs89UtmProjection(int zone) : ProjNetMapProjection(
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
        $"AUTHORITY[\"EPSG\",\"258{zone:00}\"]]")
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        public int Zone => zone;
    }

    /// <summary>
    /// NAD83 Universal Transverse Mercator Projection - EPSG:26901 to EPSG:26923.
    /// </summary>
    public class Nad83UtmProjection(int zone) : ProjNetMapProjection(
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
        $"AUTHORITY[\"EPSG\",\"269{zone:00}\"]]")
    {
        public const int FirstZone = 1;
        public const int LastZone = 23;
        public const int FirstZoneEpsgCode = 26900 + FirstZone;
        public const int LastZoneEpsgCode = 26900 + LastZone;

        public int Zone => zone;
    }

    /// <summary>
    /// NAD27 Universal Transverse Mercator Projection - EPSG:26701 to EPSG:26722.
    /// </summary>
    public class Nad27UtmProjection(int zone) : ProjNetMapProjection(
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
        $"AUTHORITY[\"EPSG\",\"267{zone:00}\"]]")
    {
        public const int FirstZone = 1;
        public const int LastZone = 22;
        public const int FirstZoneEpsgCode = 26700 + FirstZone;
        public const int LastZoneEpsgCode = 26700 + LastZone;

        public int Zone => zone;
    }

    /// <summary>
    /// ED50 Universal Transverse Mercator Projection.
    /// </summary>
    public class Ed50UtmProjection(int zone) : ProjNetMapProjection(
        $"PROJCS[\"ED50 / UTM zone {zone}N\"," +
        WktConstants.GeogCsEd50 + "," +
        "PROJECTION[\"Transverse_Mercator\"]," +
        "PARAMETER[\"latitude_of_origin\",0]," +
        $"PARAMETER[\"central_meridian\",{6 * zone - 183}]," +
        "PARAMETER[\"scale_factor\",0.9996]," +
        "PARAMETER[\"false_easting\",500000]," +
        "PARAMETER[\"false_northing\",0]," +
        "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
        "AXIS[\"Easting\",EAST]," +
        "AXIS[\"Northing\",NORTH]," +
        $"AUTHORITY[\"EPSG\",\"230{zone:00}\"]]")
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 23000 + FirstZone;
        public const int LastZoneEpsgCode = 23000 + LastZone;

        public int Zone { get; } = zone;
    }

    public class Wgs84UpsNorthProjection() : ProjNetMapProjection(
        "PROJCS[\"WGS 84 / UPS North (N,E)\"," +
        WktConstants.GeogCsWgs84 + "," +
        "PROJECTION[\"Polar_Stereographic\"]," +
        "PARAMETER[\"latitude_of_origin\",90]," +
        "PARAMETER[\"central_meridian\",0]," +
        "PARAMETER[\"scale_factor\",0.994]," +
        "PARAMETER[\"false_easting\",2000000]," +
        "PARAMETER[\"false_northing\",2000000]," +
        "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
        "AUTHORITY[\"EPSG\",\"32661\"]]")
    {
        public const string DefaultCrsId = "EPSG:32661";
    }

    public class Wgs84UpsSouthProjection() : ProjNetMapProjection(
        "PROJCS[\"WGS 84 / UPS South (N,E)\"," +
        WktConstants.GeogCsWgs84 + "," +
        "PROJECTION[\"Polar_Stereographic\"]," +
        "PARAMETER[\"latitude_of_origin\",-90]," +
        "PARAMETER[\"central_meridian\",0]," +
        "PARAMETER[\"scale_factor\",0.994]," +
        "PARAMETER[\"false_easting\",2000000]," +
        "PARAMETER[\"false_northing\",2000000]," +
        "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
        "AUTHORITY[\"EPSG\",\"32761\"]]")
    {
        public const string DefaultCrsId = "EPSG:32761";
    }
}
