using ProjNet.CoordinateSystems;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection -
    /// EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760.
    /// </summary>
    public class Wgs84UtmProjection : ProjNetMapProjection
    {
        public int Zone { get; }
        public Hemisphere Hemisphere { get; }

        public Wgs84UtmProjection(int zone, Hemisphere hemisphere)
            : base(new MapControl.Wgs84UtmProjection(zone, hemisphere))
        {
            Zone = zone;
            Hemisphere = hemisphere;
            CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(zone, hemisphere == Hemisphere.North);
        }
    }
}
