using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 UTM Projection with zone number and north/south flag.
    /// See https://en.wikipedia.org/wiki/Universal_Transverse_Mercator_coordinate_system.
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

    /// <summary>
    /// WGS84 UTM Projection with automatic zone selection from projection center.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        public Wgs84AutoUtmProjection()
            : this(MapControl.Wgs84AutoUtmProjection.DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public Wgs84AutoUtmProjection(string crsId)
            : base(31, true)
        {
            CrsId = crsId;
        }

        public override Location Center
        {
            get => base.Center;
            protected set
            {
                if (!base.Center.Equals(value))
                {
                    base.Center = value;

                    var lon = Location.NormalizeLongitude(value.Longitude);
                    var zone = (int)Math.Floor(lon / 6d) + 31;
                    var north = value.Latitude >= 0d;

                    if (Zone != zone || IsNorth != north)
                    {
                        var crsId = CrsId;
                        SetZone(zone, north);
                        CrsId = crsId;
                    }
                }
            }
        }
    }
}
