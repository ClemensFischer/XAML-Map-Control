// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System.Windows;

namespace MapControl.Projections
{
    public class AutoUtmProjection : GeoApiProjection
    {
        private int zoneNumber;
        private bool zoneIsNorth;

        public AutoUtmProjection()
        {
            UpdateZone();
        }

        public bool UseZoneCrsId { get; set; }

        public override Point LocationToMap(Location location)
        {
            UpdateZone();
            return base.LocationToMap(location);
        }

        public override Location MapToLocation(Point point)
        {
            UpdateZone();
            return base.MapToLocation(point);
        }

        private void UpdateZone()
        {
            var north = Center.Latitude >= 0d;
            var lon = Location.NormalizeLongitude(Center.Longitude);
            var zone = (int)(lon + 180d) / 6 + 1;

            if (zoneNumber != zone || zoneIsNorth != north)
            {
                zoneNumber = zone;
                zoneIsNorth = north;

                CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(zoneNumber, zoneIsNorth);

                if (!UseZoneCrsId)
                {
                    CrsId = "AUTO2:42001";
                }
            }
        }
    }
}
