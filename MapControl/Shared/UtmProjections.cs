using System;

namespace MapControl
{
    public class UtmProjection : TransverseMercatorProjection
    {
        public UtmProjection(string crsId, double equatorialRadius, double flattening, int zone, bool north = true)
            : base(equatorialRadius, flattening)
        {
            CrsId = crsId;
            ScaleFactor = 0.9996;
            CentralMeridian = zone * 6 - 183;
            FalseEasting = 5e5;
            FalseNorthing = north ? 0d : 1e7;
            Zone = zone;
        }

        public int Zone { get; }
    }

    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection -
    /// EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760.
    /// </summary>
    public class Wgs84UtmProjection : UtmProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        public Wgs84UtmProjection(int zone, bool north)
            : base($"EPSG:{(north ? 32600 : 32700) + zone}", Wgs84EquatorialRadius, Wgs84Flattening, zone, north)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }
        }
    }

    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection - EPSG:25828 to EPSG:25838.
    /// </summary>
    public class Etrs89UtmProjection : UtmProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        public Etrs89UtmProjection(int zone)
            : base($"EPSG:{25800 + zone}", 6378137d, 1d / 298.257222101, zone) // GRS 1980
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ETRS89 UTM zone {zone}.", nameof(zone));
            }
        }
    }

    /// <summary>
    /// NAD83 Universal Transverse Mercator Projection - EPSG:26901 to EPSG:26923.
    /// </summary>
    public class Nad83UtmProjection : UtmProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 23;
        public const int FirstZoneEpsgCode = 26900 + FirstZone;
        public const int LastZoneEpsgCode = 26900 + LastZone;

        public Nad83UtmProjection(int zone)
            : base($"EPSG:{26900 + zone}", 6378137d, 1d / 298.257222101, zone) // GRS 1980
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD83 UTM zone {zone}.", nameof(zone));
            }
        }
    }
}
