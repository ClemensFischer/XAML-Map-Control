// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl.Projections
{
    public class UtmProjection : GeoApiProjection
    {
        private static readonly string wktFormat = "PROJCS[\"WGS 84 / UTM zone {0}\", GEOGCS[\"WGS 84\", DATUM[\"WGS_1984\", SPHEROID[\"WGS 84\", 6378137, 298.257223563, AUTHORITY[\"EPSG\", \"7030\"]], AUTHORITY[\"EPSG\", \"6326\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.01745329251994328, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4326\"]], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], PROJECTION[\"Transverse_Mercator\"], PARAMETER[\"latitude_of_origin\", 0], PARAMETER[\"central_meridian\", {1}], PARAMETER[\"scale_factor\", 0.9996], PARAMETER[\"false_easting\", 500000], PARAMETER[\"false_northing\", {2}], AUTHORITY[\"EPSG\", \"{3}\"], AXIS[\"Easting\", EAST], AXIS[\"Northing\", NORTH]]";

        private string zoneName;

        public string ZoneName
        {
            get { return zoneName; }
            set
            {
                var zoneNumber = 0;
                var falseNorthing = 0;
                var epsgCode = 0;

                if (!string.IsNullOrEmpty(value))
                {
                    if (value.EndsWith("N"))
                    {
                        epsgCode = 32600;
                    }
                    else if (value.EndsWith("S"))
                    {
                        falseNorthing = 10000000;
                        epsgCode = 32700;
                    }

                    if (epsgCode > 0)
                    {
                        int.TryParse(value.Substring(0, value.Length - 1), out zoneNumber);
                    }
                }

                if (zoneNumber < 1 || zoneNumber > 60)
                {
                    throw new ArgumentException("Invalid UTM zone name.");
                }

                zoneName = value;
                epsgCode += zoneNumber;
                var centralMeridian = 6 * zoneNumber - 183;

                WKT = string.Format(wktFormat, zoneName, centralMeridian, falseNorthing, epsgCode);
                TrueScale = 0.9996 * MetersPerDegree;

                System.Diagnostics.Debug.WriteLine(WKT);
            }
        }
    }
}
