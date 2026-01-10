using System;

namespace MapControl
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection with automatic zone selection from
    /// the projection center. If the CRS Id passed to the constructor is null or empty,
    /// appropriate CRS Ids EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760 are used.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        public const string DefaultCrsId = "AUTO2:42001";

        private readonly string autoCrsId;

        public Wgs84AutoUtmProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
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
            protected internal set
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
