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
        private double south = double.NaN;
        private double north = double.NaN;

        public BoundingBox()
        {
        }

        public BoundingBox(double south, double west, double north, double east)
        {
            South = south;
            West = west;
            North = north;
            East = east;
        }

        public double West { get; set; } = double.NaN;
        public double East { get; set; } = double.NaN;

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

        public virtual double Width
        {
            get => East - West;
            protected set { }
        }

        public virtual double Height
        {
            get => North - South;
            protected set { }
        }

        public virtual Location Center
        {
            get => new Location((South + North) / 2d, (West + East) / 2d);
            protected set { }
        }

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
