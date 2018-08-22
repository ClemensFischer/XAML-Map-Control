using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapControl.ProjectionExtension
{
    /// <summary>
    /// projection gauss krueger (transversal mercator projection) wkt from epsg.io
    /// </summary>
    public class GaussKruegerProjection : GenericWktProjection
    {
        private const string GKConfig = "PROJCS[\"DHDN / 3-degree Gauss-Kruger zone 4\", GEOGCS[\"DHDN\", DATUM[\"Deutsches_Hauptdreiecksnetz\", SPHEROID[\"Bessel 1841\", 6377397.155, 299.1528128, AUTHORITY[\"EPSG\", \"7004\"]], TOWGS84[598.1, 73.7, 418.2, 0.202, 0.045, -2.455, 6.7], AUTHORITY[\"EPSG\", \"6314\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4314\"]], PROJECTION[\"Transverse_Mercator\"], PARAMETER[\"latitude_of_origin\", 0], PARAMETER[\"central_meridian\", 12], PARAMETER[\"scale_factor\", 1], PARAMETER[\"false_easting\", 4500000], PARAMETER[\"false_northing\", 0], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], AUTHORITY[\"EPSG\", \"31468\"]]";
        private const string GKCrsId = "EPSG:31287";

        public GaussKruegerProjection() : base(GKConfig, GKCrsId)
        {
        }
    }
}
