// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic bounding box with south and north latitude and west and east longitude values in degrees.
    /// </summary>
#if !UWP
    [System.ComponentModel.TypeConverter(typeof(BoundingBoxConverter))]
#endif
    public class BoundingBox
    {
        private double south;
        private double north;

        public BoundingBox()
        {
            south = double.NaN;
            north = double.NaN;
            West = double.NaN;
            East = double.NaN;
        }

        public BoundingBox(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            South = Math.Min(latitude1, latitude2);
            North = Math.Max(latitude1, latitude2);
            West = Math.Min(longitude1, longitude2);
            East = Math.Max(longitude1, longitude2);
        }

        public BoundingBox(Location location1, Location location2)
            : this(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude)
        {
        }

        public double South
        {
            get => south;
            set => south = Math.Min(Math.Max(value, -90d), 90d);
        }

        public double North
        {
            get => north;
            set => north = Math.Min(Math.Max(value, -90d), 90d);
        }

        public double West { get; set; }
        public double East { get; set; }

        public virtual double Width => East - West;
        public virtual double Height => North - South;

        public virtual Location Center =>
            double.IsNaN(South) || double.IsNaN(North) || double.IsNaN(West) || double.IsNaN(East)
            ? null
            : new Location((South + North) / 2d, (West + East) / 2d);

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
