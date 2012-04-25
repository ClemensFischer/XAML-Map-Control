using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Defines a normal cylindrical projection. Latitude and longitude values in degrees
    /// are transformed to cartesian coordinates with origin at latitude = 0 and longitude = 0.
    /// Longitude values are transformed identically to x values in the interval [-180 .. 180].
    /// </summary>
    public abstract class MapTransform : GeneralTransform
    {
        /// <summary>
        /// Gets the absolute value of the minimum and maximum latitude that can be transformed.
        /// </summary>
        public abstract double MaxLatitude { get; }

        /// <summary>
        /// Gets the point scale factor of the map projection at the specified point
        /// relative to the point (0, 0).
        /// </summary>
        public abstract double RelativeScale(Point point);
    }
}
