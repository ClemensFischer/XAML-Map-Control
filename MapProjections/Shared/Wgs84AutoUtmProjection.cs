using System;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection with automatic zone selection from
    /// the projection center. If the CRS identifier passed to the constructor is null or empty,
    /// appropriate values from EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760 are used.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        private readonly string autoCrsId;

        public Wgs84AutoUtmProjection() // parameterless constructor for XAML
            : this(MapControl.Wgs84AutoUtmProjection.DefaultCrsId)
        {
        }

        public Wgs84AutoUtmProjection(string crsId)
            : base(31, Hemisphere.North)
        {
            autoCrsId = crsId;

            if (!string.IsNullOrEmpty(autoCrsId))
            {
                CrsId = autoCrsId;
            }
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
                    var hemisphere = value.Latitude >= 0d ? Hemisphere.North : Hemisphere.South;

                    if (Zone != zone || Hemisphere != hemisphere)
                    {
                        SetZone(zone, hemisphere);

                        if (!string.IsNullOrEmpty(autoCrsId))
                        {
                            CrsId = autoCrsId;
                        }
                    }
                }
            }
        }
    }
}
