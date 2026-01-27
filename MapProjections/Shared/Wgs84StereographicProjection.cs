using System.Globalization;
#if WPF
using System.Windows.Media;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Stereographic Projection - AUTO2:97002.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.157-160.
    /// </summary>
    public class Wgs84StereographicProjection : ProjNetMapProjection
    {
        public Wgs84StereographicProjection()
        {
            EnableCenterUpdates();
            CenterChanged();
        }

        protected override void CenterChanged()
        {
            var wktFormat =
                "PROJCS[\"WGS 84 / World Mercator\"," +
                WktConstants.GeogCsWgs84 + "," +
                "PROJECTION[\"Oblique_Stereographic\"]," +
                "PARAMETER[\"latitude_of_origin\",{0:0.########}]," +
                "PARAMETER[\"central_meridian\",{1:0.########}]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AXIS[\"Easting\",EAST]," +
                "AXIS[\"Northing\",NORTH]" +
                "AUTHORITY[\"AUTO2\",\"97002\"]]";

            CoordinateSystemWkt = string.Format(
                CultureInfo.InvariantCulture, wktFormat, Center.Latitude, Center.Longitude);
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var p = new AzimuthalProjection.ProjectedPoint(Center.Latitude, Center.Longitude, latitude, longitude);
            var k = 2d / (1d + p.CosC); // p.157 (21-4), k0 == 1

            return p.RelativeScale(k, k);
        }
    }
}
