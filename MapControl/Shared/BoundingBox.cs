// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic bounding box with south and north latitude and west and east longitude values in degrees.
    /// </summary>
    public partial class BoundingBox
    {
        private double south;
        private double west;
        private double north;
        private double east;

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

        public double South
        {
            get { return south; }
            set { south = Math.Min(Math.Max(value, -90d), 90d); }
        }

        public double West
        {
            get { return west; }
            set { west = value; }
        }

        public double North
        {
            get { return north; }
            set { north = Math.Min(Math.Max(value, -90d), 90d); }
        }

        public double East
        {
            get { return east; }
            set { east = value; }
        }

        public virtual double Width
        {
            get { return east - west; }
        }

        public virtual double Height
        {
            get { return north - south; }
        }

        public bool HasValidBounds
        {
            get { return south < north && west < east; }
        }

        public virtual BoundingBox Clone()
        {
            return new BoundingBox(south, west, north, east);
        }

        public static BoundingBox Parse(string s)
        {
            var values = s.Split(new char[] { ',' });

            if (values.Length != 4)
            {
                throw new FormatException("BoundingBox string must be a comma-separated list of four double values");
            }

            return new BoundingBox(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture));
        }
    }
}
