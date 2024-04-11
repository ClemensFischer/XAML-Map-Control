// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic bounding box with south and north latitude and west and east longitude values in degrees.
    /// </summary>
#if WINUI || UWP
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "MapControl.BoundingBox.Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(BoundingBoxConverter))]
#endif
    public class BoundingBox
    {
        public BoundingBox()
        {
        }

        public BoundingBox(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            South = Math.Min(Math.Max(Math.Min(latitude1, latitude2), -90d), 90d);
            North = Math.Min(Math.Max(Math.Max(latitude1, latitude2), -90d), 90d);
            West = Math.Min(longitude1, longitude2);
            East = Math.Max(longitude1, longitude2);
        }

        public BoundingBox(Location location1, Location location2)
            : this(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude)
        {
        }

        public double South { get; }
        public double North { get; }
        public double West { get; }
        public double East { get; }

        public virtual double Width => East - West;
        public virtual double Height => North - South;

        public virtual Location Center => new Location((South + North) / 2d, (West + East) / 2d);

        public static BoundingBox Parse(string boundingBox)
        {
            string[] values = null;

            if (!string.IsNullOrEmpty(boundingBox))
            {
                values = boundingBox.Split(new char[] { ',' });
            }

            if (values?.Length != 4)
            {
                throw new FormatException("BoundingBox string must be a comma-separated list of four floating point numbers.");
            }

            return new BoundingBox(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture));
        }
    }
}
