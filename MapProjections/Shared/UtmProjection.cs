// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl.Projections
{
    public class UtmProjection : GeoApiProjection
    {
        private const string wktFormat = "PROJCS[\"WGS 84 / UTM zone {0}\", GEOGCS[\"WGS 84\", DATUM[\"WGS_1984\", SPHEROID[\"WGS 84\", 6378137, 298.257223563, AUTHORITY[\"EPSG\", \"7030\"]], AUTHORITY[\"EPSG\", \"6326\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.01745329251994328, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4326\"]], UNIT[\"metre\", 1, AUTHORITY[\"EPSG\", \"9001\"]], PROJECTION[\"Transverse_Mercator\"], PARAMETER[\"latitude_of_origin\", 0], PARAMETER[\"central_meridian\", {1}], PARAMETER[\"scale_factor\", 0.9996], PARAMETER[\"false_easting\", 500000], PARAMETER[\"false_northing\", {2}], AUTHORITY[\"EPSG\", \"{3}\"], AXIS[\"Easting\", EAST], AXIS[\"Northing\", NORTH]]";

        public UtmProjection()
        {
            TrueScale = 0.9996 * MetersPerDegree;
        }

        private string zone;

        public string Zone
        {
            get { return zone; }
            set
            {
                if (zone != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException("Invalid UTM zone.");
                    }

                    var hemisphere = value[value.Length - 1];
                    int zoneNumber;

                    if ((hemisphere != 'N' && hemisphere != 'S') ||
                        !int.TryParse(value.Substring(0, value.Length - 1), out zoneNumber))
                    {
                        throw new ArgumentException("Invalid UTM zone.");
                    }

                    SetZone(zoneNumber, hemisphere == 'N');
                }
            }
        }

        public void SetZone(int zoneNumber, bool north)
        {
            if (zoneNumber < 1 || zoneNumber > 60)
            {
                throw new ArgumentException("Invalid UTM zone number.");
            }

            var zoneName = zoneNumber.ToString() + (north ? "N" : "S");

            if (zone != zoneName)
            {
                var centralMeridian = zoneNumber * 6 - 183;
                var falseNorthing = north ? 0 : 10000000;
                var authorityCode = (north ? 32600 : 32700) + zoneNumber;

                zone = zoneName;
                WKT = string.Format(wktFormat, zone, centralMeridian, falseNorthing, authorityCode);
            }
        }

        public void SetZone(Location location)
        {
            var zoneNumber = Math.Min((int)(Location.NormalizeLongitude(location.Longitude) + 180d) / 6 + 1, 60);

            SetZone(zoneNumber, location.Latitude >= 0);
        }
    }
}
