#if WPF
using System.Windows.Media;
#endif

namespace MapControl.Projections
{
    public class Wgs84UpsNorthProjection : ProjNetMapProjection
    {
        public Wgs84UpsNorthProjection()
        {
            CoordinateSystemWkt =
                "PROJCS[\"WGS 84 / UPS North (N,E)\"," +
                WktConstants.GeogCsWgs84 + "," +
                "PROJECTION[\"Polar_Stereographic\"]," +
                "PARAMETER[\"latitude_of_origin\",90]," +
                "PARAMETER[\"central_meridian\",0]," +
                "PARAMETER[\"scale_factor\",0.994]," +
                "PARAMETER[\"false_easting\",2000000]," +
                "PARAMETER[\"false_northing\",2000000]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AUTHORITY[\"EPSG\",\"32661\"]]";
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var k = PolarStereographicProjection.RelativeScale(Hemisphere.North, Wgs84Flattening, 0.994, latitude);

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }
    }

    public class Wgs84UpsSouthProjection : ProjNetMapProjection
    {
        public Wgs84UpsSouthProjection()
        {
            CoordinateSystemWkt =
                "PROJCS[\"WGS 84 / UPS South (N,E)\"," +
                WktConstants.GeogCsWgs84 + "," +
                "PROJECTION[\"Polar_Stereographic\"]," +
                "PARAMETER[\"latitude_of_origin\",-90]," +
                "PARAMETER[\"central_meridian\",0]," +
                "PARAMETER[\"scale_factor\",0.994]," +
                "PARAMETER[\"false_easting\",2000000]," +
                "PARAMETER[\"false_northing\",2000000]," +
                "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                "AUTHORITY[\"EPSG\",\"32761\"]]";
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var k = PolarStereographicProjection.RelativeScale(Hemisphere.South, Wgs84Flattening, 0.994, latitude);

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }
    }
}
