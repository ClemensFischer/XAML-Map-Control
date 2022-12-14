// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 UTM Projection with zone number and north/south flag.
    /// </summary>
    public class Wgs84UtmProjection : GeoApiProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        public int Zone { get; private set; }
        public bool IsNorth { get; private set; }

        protected Wgs84UtmProjection()
        {
        }

        public Wgs84UtmProjection(int zone, bool north)
        {
            SetZone(zone, north);
        }

        protected void SetZone(int zone, bool north)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            IsNorth = north;
            CoordinateSystem = ProjectedCoordinateSystem.WGS84_UTM(Zone, IsNorth);
        }
    }

    /// <summary>
    /// WGS84 UTM Projection with automatic zone selection from projection center.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        public const string DefaultCrsId = "AUTO2:42001";

        public Wgs84AutoUtmProjection(bool useZoneCrsId = false)
        {
            UseZoneCrsId = useZoneCrsId;
            UpdateZone();
        }

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

            if (Zone != zone || IsNorth != north || string.IsNullOrEmpty(CrsId))
            {
                SetZone(zone, north);

                if (!UseZoneCrsId)
                {
                    CrsId = DefaultCrsId;
                }
            }
        }
    }
}
