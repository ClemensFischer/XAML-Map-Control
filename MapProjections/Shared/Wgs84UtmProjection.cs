// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    public class Wgs84UtmProjection : GeoApiProjection
    {
        public Wgs84UtmProjection(int zone, bool north)
        {
            SetZone(zone, north);
        }

        protected void SetZone(int zone, bool north)
        {
            if (zone < 1 || zone > 60)
            {
                throw new ArgumentException($"Invalid UTM zone {zone}.", nameof(zone));
            }

            CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
        }
    }
}
