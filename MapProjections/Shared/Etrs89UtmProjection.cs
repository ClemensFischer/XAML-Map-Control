using System;

namespace MapControl.Projections
{
    /// <summary>
    /// ETRS89 Universal Transverse Mercator Projection.
    /// </summary>
    public class Etrs89UtmProjection : GeoApiProjection
    {
        public const int FirstZone = 28;
        public const int LastZone = 38;
        public const int FirstZoneEpsgCode = 25800 + FirstZone;
        public const int LastZoneEpsgCode = 25800 + LastZone;

        public int Zone { get; }

        public Etrs89UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid ETRS89 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt
                = $"PROJCS[\"ETRS89 / UTM zone {zone}N\","
                + GeoApiProjectionFactory.GeoGcsEtrs89 + ","
                + GeoApiProjectionFactory.ProjectionTransverseMercator + ","
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + GeoApiProjectionFactory.UnitMeter + ","
                + GeoApiProjectionFactory.AxisEasting + ","
                + GeoApiProjectionFactory.AxisNorthing + ","
                + $"AUTHORITY[\"EPSG\",\"258{zone:00}\"]]";
        }
    }
}
