using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapControl.ProjectionExtension
{
    public class AustriaLambertProjection : GenericWktProjection
    {
        private const string ALConfig = "PROJCS[\"MGI / Austria Lambert\",GEOGCS[\"MGI\",DATUM[\"Militar_Geographische_Institute\",SPHEROID[\"Bessel 1841\", 6377397.155, 299.1528128, AUTHORITY[\"EPSG\", \"7004\"]],TOWGS84[577.326, 90.129, 463.919, 5.137, 1.474, 5.297, 2.4232], AUTHORITY[\"EPSG\", \"6312\"]],PRIMEM[\"Greenwich\", 0,AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433,AUTHORITY[\"EPSG\", \"9122\"]],AUTHORITY[\"EPSG\", \"4312\"]], PROJECTION[\"Lambert_Conformal_Conic_2SP\"],PARAMETER[\"standard_parallel_1\", 49], PARAMETER[\"standard_parallel_2\", 46],PARAMETER[\"latitude_of_origin\", 47.5],PARAMETER[\"central_meridian\", 13.33333333333333], PARAMETER[\"false_easting\", 400000],PARAMETER[\"false_northing\", 400000],UNIT[\"metre\", 1,AUTHORITY[\"EPSG\", \"9001\"]],AUTHORITY[\"EPSG\", \"31287\"]]";
        private const string ALCrsId = "EPSG:31287";

        public AustriaLambertProjection() : base(ALConfig, ALCrsId)
        {
        }
    }
}
