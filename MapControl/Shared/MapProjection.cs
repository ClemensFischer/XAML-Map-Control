﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WPF
using System.Windows;
#endif

namespace MapControl
{
    public enum MapProjectionType
    {
        WebMercator, // normal cylindrical projection compatible with MapTileLayer
        NormalCylindrical,
        TransverseCylindrical,
        Azimuthal,
        Other
    }

    /// <summary>
    /// Defines a map projection between geographic coordinates and cartesian map coordinates.
    /// </summary>
    public abstract class MapProjection
    {
        public const double Wgs84EquatorialRadius = 6378137d;
        public const double Wgs84MeterPerDegree = Wgs84EquatorialRadius * Math.PI / 180d;
        public const double Wgs84Flattening = 1d / 298.257223563;
        public static readonly double Wgs84Eccentricity = Math.Sqrt((2d - Wgs84Flattening) * Wgs84Flattening);

        /// <summary>
        /// Gets the type of the projection.
        /// </summary>
        public MapProjectionType Type { get; protected set; } = MapProjectionType.Other;

        /// <summary>
        /// Gets the WMS 1.3.0 CRS identifier.
        /// </summary>
        public string CrsId { get; protected set; } = "";

        /// <summary>
        /// Gets or sets an optional projection center.
        /// </summary>
        public virtual Location Center { get; protected internal set; } = new Location();

        /// <summary>
        /// Gets the relative map scale at the specified Location.
        /// </summary>
        public virtual Point GetRelativeScale(Location location) => new Point(1d, 1d);

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in projected map coordinates.
        /// Returns null when the Location can not be transformed.
        /// </summary>
        public abstract Point? LocationToMap(Location location);

        /// <summary>
        /// Transforms a Point in projected map coordinates to a Location in geographic coordinates.
        /// Returns null when the Point can not be transformed.
        /// </summary>
        public abstract Location MapToLocation(Point point);

        /// <summary>
        /// Transforms a BoundingBox in geographic coordinates to a Rect in projected map coordinates.
        /// Returns null when the BoundingBox can not be transformed.
        /// </summary>
        public virtual Rect? BoundingBoxToMap(BoundingBox boundingBox)
        {
            Rect? rect = null;

            if (boundingBox.South < boundingBox.North && boundingBox.West < boundingBox.East)
            {
                var p1 = LocationToMap(new Location(boundingBox.South, boundingBox.West));
                var p2 = LocationToMap(new Location(boundingBox.North, boundingBox.East));

                if (p1.HasValue && p2.HasValue)
                {
                    rect = new Rect(p1.Value, p2.Value);
                }
            }
            else if (boundingBox.Center != null)
            {
                // boundingBox is a CenteredBoundingBox
                //
                var center = LocationToMap(boundingBox.Center);

                if (center.HasValue)
                {
                    var width = boundingBox.Width * Wgs84MeterPerDegree;
                    var height = boundingBox.Height * Wgs84MeterPerDegree;
                    var x = center.Value.X - width / 2d;
                    var y = center.Value.Y - height / 2d;

                    rect = new Rect(x, y, width, height);
                }
            }

            return rect;
        }

        /// <summary>
        /// Transforms a Rect in projected map coordinates to a BoundingBox in geographic coordinates.
        /// Returns null when the MapRect can not be transformed.
        /// </summary>
        public virtual BoundingBox MapToBoundingBox(Rect rect)
        {
            var southWest = MapToLocation(new Point(rect.X, rect.Y));
            var northEast = MapToLocation(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            return southWest != null && northEast != null
                ? new BoundingBox(southWest, northEast)
                : null;
        }

        /// <summary>
        /// Gets the CRS parameter value for a WMS GetMap request.
        /// </summary>
        public virtual string GetCrsValue()
        {
            return CrsId.StartsWith("AUTO2:") || CrsId.StartsWith("AUTO:")
                ? string.Format(CultureInfo.InvariantCulture, "{0},1,{1},{2}", CrsId, Center.Longitude, Center.Latitude)
                : CrsId;
        }

        /// <summary>
        /// Gets the BBOX parameter value for a WMS GetMap request.
        /// </summary>
        public virtual string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0:F2},{1:F2},{2:F2},{3:F2}",
                rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }
    }
}
