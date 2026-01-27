using ProjNet.CoordinateSystems;
using System;
#if WPF
using System.Windows.Media;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Mercator Projection implemented by setting the CoordinateSystem property of a ProjNetMapProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection : ProjNetMapProjection
    {
        public WebMercatorProjection()
        {
            CoordinateSystem = ProjectedCoordinateSystem.WebMercator;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var k = 1d / Math.Cos(latitude * Math.PI / 180d); // p.44 (7-3)

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }
    }
}
