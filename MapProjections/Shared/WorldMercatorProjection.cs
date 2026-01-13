using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// Elliptical Mercator Projection implemented by setting the WKT property of a GeoApiProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44-45.
    /// </summary>
    public class WorldMercatorProjection : GeoApiProjection
    {
        public WorldMercatorProjection()
        {
            CoordinateSystemWkt
                = "PROJCS[\"WGS 84 / World Mercator\","
                + "GEOGCS[\"WGS 84\","
                + "DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563]],"
                + GeoApiProjectionFactory.PrimeMeridian + ","
                + GeoApiProjectionFactory.UnitDegree + ","
                + "AUTHORITY[\"EPSG\",\"4326\"]],"
                + "PROJECTION[\"Mercator_1SP\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + "PARAMETER[\"central_meridian\",0],"
                + "PARAMETER[\"scale_factor\",1],"
                + "PARAMETER[\"false_easting\",0],"
                + "PARAMETER[\"false_northing\",0],"
                + GeoApiProjectionFactory.UnitMeter + ","
                + GeoApiProjectionFactory.AxisEast + ","
                + GeoApiProjectionFactory.AxisNorth + ","
                + "AUTHORITY[\"EPSG\",\"3395\"]]";
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            var lat = latitude * Math.PI / 180d;
            var eSinLat = MapControl.WorldMercatorProjection.Wgs84Eccentricity * Math.Sin(lat);
            var k = Math.Sqrt(1d - eSinLat * eSinLat) / Math.Cos(lat); // p.44 (7-8)

            return new Point(k, k);
        }
    }
}
