using System;

namespace MapControl
{
    /// <summary>
    /// WGS84 Universal Transverse Mercator Projection - AUTO2:42002.
    /// Zone and Hemisphere are automatically set according to the projection's Center.
    /// If the CRS identifier passed to the constructor is null or empty, appropriate
    /// values from EPSG:32601 to EPSG:32660 and EPSG:32701 to EPSG:32760 are used.
    /// </summary>
    public class Wgs84AutoUtmProjection : Wgs84UtmProjection
    {
        public const string DefaultCrsId = "AUTO2:42001";

        private readonly string autoCrsId;

        public Wgs84AutoUtmProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
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

        protected override void CenterChanged()
        {
            var zone = (int)Math.Floor(Center.Longitude / 6d) + 31;
            var hemisphere = Center.Latitude >= 0d ? Hemisphere.North : Hemisphere.South;

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
