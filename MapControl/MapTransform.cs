// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Defines a normal cylindrical projection. Latitude and longitude values in degrees are
    /// transformed to cartesian coordinates with origin at latitude = 0 and longitude = 0.
    /// Longitude values are transformed identically to x values in the interval [-180 .. 180].
    /// Latitude values in the interval [-MaxLatitude .. MaxLatitude] are transformed to y values in
    /// the interval [-180 .. 180] according to the actual projection, e.g. the Mercator projection.
    /// </summary>
    public abstract class MapTransform
    {
        /// <summary>
        /// Gets the absolute value of the minimum and maximum latitude that can be transformed.
        /// </summary>
        public abstract double MaxLatitude { get; }

        /// <summary>
        /// Gets the scale factor of the map projection at the specified geographic location
        /// relative to the scale at latitude zero.
        /// </summary>
        public abstract double RelativeScale(Location location);

        /// <summary>
        /// Transforms a geographic location to a cartesian coordinate point.
        /// </summary>
        public abstract Point Transform(Location location);

        /// <summary>
        /// Transforms a cartesian coordinate point to a geographic location.
        /// </summary>
        public abstract Location Transform(Point point);
    }
}
