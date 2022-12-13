// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl.Projections
{
    public class Ed50UtmProjection : GeoApiProjection
    {
        public Ed50UtmProjection(int zone)
        {
            if (zone < 28 || zone > 38)
            {
                throw new ArgumentException($"Invalid UTM zone {zone}.", nameof(zone));
            }

            CoordinateSystemWkt
                = $"PROJCS[\"ED50 / UTM zone {zone}N\","
                + "GEOGCS[\"ED50\","
                + "DATUM[\"European_Datum_1950\","
                + "SPHEROID[\"International 1924\",6378388,297,"
                + "AUTHORITY[\"EPSG\",\"7022\"]],"
                + "TOWGS84[-87,-98,-121,0,0,0,0],"
                + "AUTHORITY[\"EPSG\",\"6230\"]],"
                + "PRIMEM[\"Greenwich\",0,"
                + "AUTHORITY[\"EPSG\",\"8901\"]],"
                + "UNIT[\"degree\",0.0174532925199433,"
                + "AUTHORITY[\"EPSG\",\"9122\"]],"
                + "AUTHORITY[\"EPSG\",\"4230\"]],"
                + "PROJECTION[\"Transverse_Mercator\"],"
                + "PARAMETER[\"latitude_of_origin\",0],"
                + $"PARAMETER[\"central_meridian\",{6 * zone - 183}],"
                + "PARAMETER[\"scale_factor\",0.9996],"
                + "PARAMETER[\"false_easting\",500000],"
                + "PARAMETER[\"false_northing\",0],"
                + "UNIT[\"metre\",1,"
                + "AUTHORITY[\"EPSG\",\"9001\"]],"
                + "AXIS[\"Easting\",EAST],"
                + "AXIS[\"Northing\",NORTH],"
                + $"AUTHORITY[\"EPSG\",\"230{zone}\"]]";
        }
    }
}
