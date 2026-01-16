
using System.Globalization;

namespace MapControl.Projections
{
    public class Wgs84OrthographicProjection : ProjNetMapProjection
    {
        public Wgs84OrthographicProjection()
        {
            Center = base.Center;
        }

        public override Location Center
        {
            get => base.Center;
            protected set
            {
                base.Center = value;

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
                    CultureInfo.InvariantCulture, wktFormat, value.Latitude, value.Longitude);
            }
        }
    }
}
