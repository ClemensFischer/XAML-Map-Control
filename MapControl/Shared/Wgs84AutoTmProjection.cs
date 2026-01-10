namespace MapControl
{
    /// <summary>
    /// WGS84 Auto Transverse Mercator Projection.
    /// </summary>
    public class Wgs84AutoTmProjection : TransverseMercatorProjection
    {
        public const string DefaultCrsId = "AUTO2:42002";

        public Wgs84AutoTmProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public Wgs84AutoTmProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Location Center
        {
            get => base.Center;
            protected internal set
            {
                base.Center = value;
                CentralMeridian = value.Longitude;
            }
        }
    }
}
