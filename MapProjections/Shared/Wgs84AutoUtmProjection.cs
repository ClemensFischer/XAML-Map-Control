// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    public class Wgs84AutoUtmProjection : GeoApiProjection
    {
        public const string DefaultCrsId = "AUTO2:42001";

        public Wgs84AutoUtmProjection(bool useZoneCrsId = false)
        {
            UseZoneCrsId = useZoneCrsId;
            UpdateZone();
        }

        public int Zone { get; private set; }
        public bool IsNorth { get; private set; }
        public bool UseZoneCrsId { get; }

        public override Location Center
        {
            get => base.Center;
            set
            {
                if (!Equals(base.Center, value))
                {
                    base.Center = value;
                    UpdateZone();
                }
            }
        }

        private void UpdateZone()
        {
            var lon = Location.NormalizeLongitude(Center.Longitude);
            var zone = (int)Math.Floor(lon / 6d) + 31;
            var north = Center.Latitude >= 0d;

            if (Zone != zone || IsNorth != north)
            {
                Zone = zone;
                IsNorth = north;
                CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(Zone, IsNorth);

                if (!UseZoneCrsId)
                {
                    CrsId = DefaultCrsId;
                }
            }
        }
    }
}
