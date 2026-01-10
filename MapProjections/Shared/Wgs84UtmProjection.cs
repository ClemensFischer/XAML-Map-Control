using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection.
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
}
