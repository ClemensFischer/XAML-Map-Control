#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl.Projections
{
    public class Wgs84UpsNorthProjection : ProjNetMapProjection
    {
        public Wgs84UpsNorthProjection()
        {
            CoordinateSystemWkt
                = "PROJCS[\"WGS 84 / UPS North (N,E)\","
                + ProjNetMapProjectionFactory.GeoGcsWgs84 + ","
                + "PROJECTION[\"Polar_Stereographic\"],"
                + "PARAMETER[\"latitude_of_origin\",90],"
                + "PARAMETER[\"central_meridian\",0],"
                + "PARAMETER[\"scale_factor\",0.994],"
                + "PARAMETER[\"false_easting\",2000000],"
                + "PARAMETER[\"false_northing\",2000000],"
                + ProjNetMapProjectionFactory.UnitMeter + ","
                + "AUTHORITY[\"EPSG\",\"32661\"]]";

            Type = MapProjectionType.Azimuthal;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            var k = PolarStereographicProjection.RelativeScale(Hemisphere.North, Wgs84EquatorialRadius, Wgs84Flattening, 0.994, latitude);

            return new Point(k, k);
        }
    }

    public class Wgs84UpsSouthProjection : ProjNetMapProjection
    {
        public Wgs84UpsSouthProjection()
        {
            CoordinateSystemWkt
                = "PROJCS[\"WGS 84 / UPS South (N,E)\","
                + ProjNetMapProjectionFactory.GeoGcsWgs84 + ","
                + "PROJECTION[\"Polar_Stereographic\"],"
                + "PARAMETER[\"latitude_of_origin\",-90],"
                + "PARAMETER[\"central_meridian\",0],"
                + "PARAMETER[\"scale_factor\",0.994],"
                + "PARAMETER[\"false_easting\",2000000],"
                + "PARAMETER[\"false_northing\",2000000],"
                + ProjNetMapProjectionFactory.UnitMeter + ","
                + "AUTHORITY[\"EPSG\",\"32761\"]]";

            Type = MapProjectionType.Azimuthal;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            var k = PolarStereographicProjection.RelativeScale(Hemisphere.South, Wgs84EquatorialRadius, Wgs84Flattening, 0.994, latitude);

            return new Point(k, k);
        }
    }
}
