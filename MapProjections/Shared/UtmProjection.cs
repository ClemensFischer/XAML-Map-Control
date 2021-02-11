// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using ProjNet.CoordinateSystems;

namespace MapControl.Projections
{
    public class UtmProjection : GeoApiProjection
    {
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

                    if ((hemisphere != 'N' && hemisphere != 'S') ||
                        !int.TryParse(value.Substring(0, value.Length - 1), out int zoneNumber))
                    {
                        throw new ArgumentException("Invalid UTM zone.");
                    }

                    SetZone(zoneNumber, hemisphere == 'N');
                }
            }
        }

        public void SetZone(int zoneNumber, bool north)
        {
            if (zoneNumber < 1 || zoneNumber > 61)
            {
                throw new ArgumentException("Invalid UTM zone number.", nameof(zoneNumber));
            }

            var zoneName = zoneNumber.ToString() + (north ? "N" : "S");

            if (zone != zoneName)
            {
                zone = zoneName;
                CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(zoneNumber, north);
            }
        }

        public void SetZone(Location location)
        {
            var zoneNumber = Math.Min((int)(Location.NormalizeLongitude(location.Longitude) + 180d) / 6 + 1, 60);

            SetZone(zoneNumber, location.Latitude >= 0);
        }
    }
}
