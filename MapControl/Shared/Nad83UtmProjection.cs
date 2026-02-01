using System;

namespace MapControl
{
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

        public Nad83UtmProjection(int zone) : base(zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD83 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{26900 + zone}";

            // GRS 1980
            EquatorialRadius = 6378137d;
            Flattening = 1d / 298.257222101;
        }
    }
}
