using System;

namespace MapControl.Projections
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection with
    /// automatic zone selection from projection center.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        private readonly string autoCrsId;

        public Wgs84AutoUtmProjection()
            : this(MapControl.Wgs84AutoUtmProjection.DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        /// <summary>
        /// When the crsId parameter is null or empty, the projection will use EPSG:32***.
        /// </summary>
        public Wgs84AutoUtmProjection(string crsId)
            : base(31, true)
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
                    var north = value.Latitude >= 0d;

                    if (Zone != zone || IsNorth != north)
                    {
                        SetZone(zone, north);

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
