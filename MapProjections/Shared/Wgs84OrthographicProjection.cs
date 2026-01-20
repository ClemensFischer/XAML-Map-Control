using System;
using System.Globalization;
#if WPF
using System.Windows.Media;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Orthographic Projection - AUTO2:42003.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.148-150.
    /// </summary>
    public class Wgs84OrthographicProjection : ProjNetMapProjection
    {
        public Wgs84OrthographicProjection()
        {
            CenterChanged();
        }

        protected override void CenterChanged()
        {
            var wktFormat =
                    "PROJCS[\"WGS 84 / World Mercator\"," +
                    WktConstants.GeogCsWgs84 + "," +
                    "PROJECTION[\"Orthographic\"]," +
                    "PARAMETER[\"latitude_of_origin\",{0:0.########}]," +
                    "PARAMETER[\"central_meridian\",{1:0.########}]," +
                    "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                    "AXIS[\"Easting\",EAST]," +
                    "AXIS[\"Northing\",NORTH]" +
                    "AUTHORITY[\"AUTO2\",\"42003\"]]";

            CoordinateSystemWkt = string.Format(
                CultureInfo.InvariantCulture, wktFormat, Center.Latitude, Center.Longitude);
        }

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            var p = new AzimuthalProjection.ProjectedPoint(Center.Latitude, Center.Longitude, latitude, longitude);
            var h = p.CosC; // p.149 (20-5)

            var scale = new Matrix(h, 0d, 0d, 1d, 0d, 0d);
            scale.Rotate(-Math.Atan2(p.Y, p.X) * 180d / Math.PI);
            return scale;
        }
    }
}
