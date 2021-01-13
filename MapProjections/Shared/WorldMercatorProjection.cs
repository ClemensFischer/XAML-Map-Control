// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
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
            WKT = "PROJCS[\"WGS 84 / World Mercator\"," +
                "GEOGCS[\"WGS 84\"," +
                    "DATUM[\"WGS_1984\"," +
                        "SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]]," +
                        "AUTHORITY[\"EPSG\",\"6326\"]]," +
                    "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]]," +
                    "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]]," +
                    "AUTHORITY[\"EPSG\",\"4326\"]]," +
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

        public override Vector GetRelativeScale(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);
            var k = Math.Sqrt(1d - eSinLat * eSinLat) / Math.Cos(lat); // p.44 (7-8)

            return new Vector(k, k);
        }
    }
}
