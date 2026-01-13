using System;

namespace MapControl.Projections
{
    /// <summary>
    /// NAD27 Universal Transverse Mercator Projection.
    /// </summary>
    public class Nad27UtmProjection : GeoApiProjection
    {
        public const int FirstZone = 1;
        public const int LastZone = 22;
        public const int FirstZoneEpsgCode = 26700 + FirstZone;
        public const int LastZoneEpsgCode = 26700 + LastZone;

        public int Zone { get; }

        public Nad27UtmProjection(int zone)
        {
            if (zone < FirstZone || zone > LastZone)
            {
                throw new ArgumentException($"Invalid NAD27 UTM zone {zone}.", nameof(zone));
            }

            Zone = zone;
            CoordinateSystemWkt
                = $"PROJCS[\"NAD27 / UTM zone {zone}N\","
                + "GEOGCS[\"NAD27\","
                + "DATUM[\"North_American_Datum_1927\","
                + "SPHEROID[\"Clarke 1866\",6378206.4,294.978698213898]],"
                + GeoApiProjectionFactory.PrimeMeridianGreenwich + ","
                + GeoApiProjectionFactory.UnitDegree + ","
                + "AUTHORITY[\"EPSG\",\"4267\"]],"
                + GeoApiProjectionFactory.ProjectionTransverseMercator + ","
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + GeoApiProjectionFactory.UnitMeter + ","
                + GeoApiProjectionFactory.AxisEasting + ","
                + GeoApiProjectionFactory.AxisNorthing + ","
                + $"AUTHORITY[\"EPSG\",\"267{zone:00}\"]]";
        }
    }
}
