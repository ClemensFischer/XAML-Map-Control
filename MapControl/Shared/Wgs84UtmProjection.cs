using System;

namespace MapControl
{
    public enum Hemisphere
    {
        North,
        South
    }

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
        public Hemisphere Hemisphere { get; }

        public Wgs84UtmProjection(int zone, Hemisphere hemisphere)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            Hemisphere = hemisphere;
            CrsId = $"EPSG:{(hemisphere == Hemisphere.North ? 32600 : 32700) + zone}";
            CentralMeridian = zone * 6d - 183d;
            FalseNorthing = hemisphere == Hemisphere.North ? 0d : 1e7;
        }
    }
}
