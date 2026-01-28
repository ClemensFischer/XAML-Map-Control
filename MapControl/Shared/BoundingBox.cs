using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic bounding box with south and north latitude and west and east longitude values in degrees.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(BoundingBoxConverter))]
#endif
    public class BoundingBox(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        public double South { get; } = Math.Min(Math.Max(Math.Min(latitude1, latitude2), -90d), 90d);
        public double North { get; } = Math.Min(Math.Max(Math.Max(latitude1, latitude2), -90d), 90d);
        public double West { get; } = Math.Min(longitude1, longitude2);
        public double East { get; } = Math.Max(longitude1, longitude2);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", South, West, North, East);
        }

        /// <summary>
        /// Creates a BoundingBox instance from a string containing a comma-separated sequence of four floating point numbers.
        /// </summary>
        public static BoundingBox Parse(string boundingBox)
        {
            string[] values = null;

            if (!string.IsNullOrEmpty(boundingBox))
            {
                values = boundingBox.Split(',');
            }

            if (values == null || values.Length != 4 && values.Length != 5)
            {
                throw new FormatException($"{nameof(BoundingBox)} string must contain a comma-separated sequence of four floating point numbers.");
            }

            return new BoundingBox(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture));
        }
    }
}
