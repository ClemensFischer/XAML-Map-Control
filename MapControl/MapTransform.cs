// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Defines a normal cylindrical projection. Latitude and longitude values in degrees
    /// are transformed to cartesian coordinates with origin at latitude = 0 and longitude = 0.
    /// Longitude values are transformed identically to x values in the interval [-180 .. 180].
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
        public abstract Location TransformBack(Point point);

        /// <summary>
        /// Transforms a collection of geographic locations to a collection of cartesian coordinate points.
        /// </summary>
        public PointCollection Transform(IEnumerable<Location> locations)
        {
            return new PointCollection(locations.Select(l => Transform(l)));
        }

        /// <summary>
        /// Transforms a collection of cartesian coordinate points to a collections of geographic location.
        /// </summary>
        public LocationCollection TransformBack(IEnumerable<Point> points)
        {
            return new LocationCollection(points.Select(p => TransformBack(p)));
        }
    }
}
