using System;

namespace MapControl
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection.
    /// </summary>
    public class Wgs84UtmProjection : TransverseMercatorProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 60;
        public const int FirstZoneNorthEpsgCode = 32600 + FirstZone;
        public const int LastZoneNorthEpsgCode = 32600 + LastZone;
        public const int FirstZoneSouthEpsgCode = 32700 + FirstZone;
        public const int LastZoneSouthEpsgCode = 32700 + LastZone;

        public int Zone { get; private set; }
        public bool IsNorth { get; private set; }

        public Wgs84UtmProjection(int zone, bool north)
        {
            SetZone(zone, north);

            EquatorialRadius = Wgs84EquatorialRadius;
            Flattening = Wgs84Flattening;
            ScaleFactor = 0.9996;
            FalseEasting = 5e5;
        }

        protected void SetZone(int zone, bool north)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid WGS84 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            IsNorth = north;
            CrsId = $"EPSG:{(north ? 32600 : 32700) + zone}";
            CentralMeridian = zone * 6d - 183d;
            FalseNorthing = north ? 0d : 1e7;
        }
    }
}
