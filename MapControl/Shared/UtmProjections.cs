using System;

namespace MapControl
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection -
    /// EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760.
    /// </summary>
    public class Wgs84UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        public int Zone { get; }

        public Wgs84UtmProjection(int zone, bool north)
            : base(Wgs84EquatorialRadius, Wgs84Flattening, 0.9996, zone, north)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{(north ? 32600 : 32700) + zone}";
        }
    }

    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection - EPSG:25828 to EPSG:25838.
    /// </summary>
    public class Etrs89UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        public int Zone { get; }

        public Etrs89UtmProjection(int zone)
            : base(6378137d, 1d / 298.257222101, 0.9996, zone) // GRS 1980
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ETRS89 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{25800 + zone}";
        }
    }

    /// <summary>
    /// NAD83 Universal Transverse Mercator Projection - EPSG:26901 to EPSG:26923.
    /// </summary>
    public class Nad83UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 23;
        public const int FirstZoneEpsgCode = 26900 + FirstZone;
        public const int LastZoneEpsgCode = 26900 + LastZone;

        public int Zone { get; }

        public Nad83UtmProjection(int zone)
            : base(6378137d, 1d / 298.257222101, 0.9996, zone) // GRS 1980
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD83 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{26900 + zone}";
        }
    }

    /// <summary>
    /// NAD27 Universal Transverse Mercator Projection - EPSG:26701 to EPSG:26722.
    /// </summary>
    public class Nad27UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 22;
        public const int FirstZoneEpsgCode = 26700 + FirstZone;
        public const int LastZoneEpsgCode = 26700 + LastZone;

        public int Zone { get; }

        public Nad27UtmProjection(int zone)
            : base(6378206.4, 1d / 294.978698213898, 0.9996, zone) // Clarke 1866
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD27 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{26700 + zone}";
        }
    }
}
