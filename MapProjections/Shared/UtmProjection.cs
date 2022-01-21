// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    public class UtmProjection : GeoApiProjection
    {
        public UtmProjection(int zone, bool north)
        {
            SetZone(zone, north);
        }

        public UtmProjection(Location location)
        {
            var zone = Math.Min((int)Math.Floor(Location.NormalizeLongitude(location.Longitude) + 180d) / 6 + 1, 60);

            SetZone(zone, location.Latitude >= 0d);
        }

        protected void SetZone(int zone, bool north)
        {
            if (zone < 1 || zone > 60)
            {
                throw new ArgumentException("Invalid UTM zone number.", nameof(zone));
            }

            CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
        }
    }
}
