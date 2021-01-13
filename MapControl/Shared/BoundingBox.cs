// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic bounding box with south and north latitude and west and east longitude values in degrees.
    /// </summary>
#if !WINDOWS_UWP
    [System.ComponentModel.TypeConverter(typeof(BoundingBoxConverter))]
#endif
    public class BoundingBox
    {
        private double south;
        private double north;

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

        public double West { get; set; }

        public double East { get; set; }

        public double South
        {
            get { return south; }
            set { south = Math.Min(Math.Max(value, -90d), 90d); }
        }

        public double North
        {
            get { return north; }
            set { north = Math.Min(Math.Max(value, -90d), 90d); }
        }

        public virtual double Width
        {
            get { return East - West; }
        }

        public virtual double Height
        {
            get { return North - South; }
        }

        public virtual BoundingBox Clone()
        {
            return new BoundingBox(South, West, North, East);
        }

        public static BoundingBox Parse(string s)
        {
            var values = s.Split(new char[] { ',' });

            if (values.Length != 4)
            {
                throw new FormatException("BoundingBox string must be a comma-separated list of four double values.");
            }

            return new BoundingBox(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture));
        }
    }
}
