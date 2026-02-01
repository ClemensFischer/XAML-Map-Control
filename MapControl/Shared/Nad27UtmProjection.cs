using System;

namespace MapControl
{
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

        public Nad27UtmProjection(int zone) : base(zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD27 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{26700 + zone}";

            // Clarke 1866
            EquatorialRadius = 6378206.4;
            Flattening = 1d / 294.978698213898;
        }
    }
}
