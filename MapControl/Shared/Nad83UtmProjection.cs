// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// NAD83 UTM Projection with zone number.
    /// </summary>
    public class Nad83UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 23;
        public const int FirstZoneEpsgCode = 26900 + FirstZone;
        public const int LastZoneEpsgCode = 26900 + LastZone;

        public int Zone { get; }

        public Nad83UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD83 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CrsId = $"EPSG:{Zone - FirstZone + FirstZoneEpsgCode}";

            // GRS 1980
            EquatorialRadius = 6378137d;
            Flattening = 1d / 298.257222101;
            ScaleFactor = DefaultScaleFactor;
            CentralMeridian = Zone * 6d - 183d;
            FalseEasting = 5e5;
            FalseNorthing = 0d;
        }
    }
}
