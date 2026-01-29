using ProjNet.CoordinateSystems;

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Mercator Projection implemented by setting the CoordinateSystem property of a ProjNetMapProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection : ProjNetMapProjection
    {
        public WebMercatorProjection()
            : base(new MapControl.WebMercatorProjection())
        {
            CoordinateSystem = ProjectedCoordinateSystem.WebMercator;
        }
    }
}
