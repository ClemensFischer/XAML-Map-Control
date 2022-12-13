// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// ETRS89 UTM Projection with zone number.
    /// </summary>
    public class Etrs89UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        private int zone;

        public Etrs89UtmProjection(int zone)
        {
            Zone = zone;
        }

        public int Zone
        {
            get => zone;
            set
            {
                if (value < FirstZone || value > LastZone)
                {
                    throw new ArgumentException($"Invalid ETRS89 UTM zone {value}.", nameof(value));
                }

                zone = value;
                CrsId = $"EPSG:{zone - FirstZone + FirstZoneEpsgCode}";
                EquatorialRadius = 6378137d;
                Flattening = 1 / 298.257222101; // GRS 1980
                ScaleFactor = 0.9996;
                CentralMeridian = zone * 6d - 183d;
                FalseEasting = 5e5;
                FalseNorthing = 0d;
            }
        }
    }

    /// <summary>
    /// WGS84 UTM Projection with zone number and north/south flag.
    /// </summary>
    public class Wgs84UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        protected Wgs84UtmProjection()
        {
        }

        public Wgs84UtmProjection(int zone, bool north)
        {
            SetZone(zone, north);
        }

        public int Zone { get; private set; }
        public bool IsNorth { get; private set; }

        public void SetZone(int zone, bool north, string crsId = null)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }

            var epsgCode = zone - FirstZone + (north ? FirstZoneNorthEpsgCode : FirstZoneSouthEpsgCode);

            Zone = zone;
            IsNorth = north;
            CrsId = crsId ?? $"EPSG:{epsgCode}";
            ScaleFactor = 0.9996;
            CentralMeridian = Zone * 6d - 183d;
            FalseEasting = 5e5;
            FalseNorthing = IsNorth ? 0d : 1e7;
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
                SetZone(zone, north, UseZoneCrsId ? null : DefaultCrsId);
            }
        }

    }
}
