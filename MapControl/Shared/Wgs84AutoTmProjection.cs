namespace MapControl
{
    /// <summary>
    /// WGS84 Auto Transverse Mercator Projection - AUTO2:42002.
    /// CentralMeridian is automatically set to Center.Longitude.
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

        protected override void CenterChanged()
        {
            CentralMeridian = Center.Longitude;
        }
    }
}
