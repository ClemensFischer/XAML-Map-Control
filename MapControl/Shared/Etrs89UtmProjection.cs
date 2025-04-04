﻿using System;

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

        public int Zone { get; }

        public Etrs89UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ETRS89 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{25800 + Zone}";

            // GRS 1980
            EquatorialRadius = 6378137d;
            Flattening = 1d / 298.257222101;
            ScaleFactor = 0.9996;
            CentralMeridian = zone * 6d - 183d;
            FalseEasting = 5e5;
            FalseNorthing = 0d;
        }
    }
}
