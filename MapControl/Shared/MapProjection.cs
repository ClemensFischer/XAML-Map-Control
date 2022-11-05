// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINUI || UWP
using Windows.Foundation;
#else
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

        public static MapProjectionFactory Factory { get; set; } = new MapProjectionFactory();

        /// <summary>
        /// Gets the type of the projection.
        /// </summary>
        public MapProjectionType Type { get; protected set; } = MapProjectionType.Other;

        /// <summary>
        /// Gets the WMS 1.3.0 CRS identifier.
        /// </summary>
        public string CrsId { get; protected set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional projection center.
        /// </summary>
        public Location Center { get; set; } = new Location();

        /// <summary>
        /// Gets the relative map scale at the specified Location.
        /// </summary>
        public virtual Vector GetRelativeScale(Location location)
        {
            return new Vector(1d, 1d);
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in projected map coordinates.
        /// </summary>
        public abstract Point LocationToMap(Location location);

        /// <summary>
        /// Transforms a Point in projected map coordinates to a Location in geographic coordinates.
        /// Returns null when the Point can not be transformed.
        /// </summary>
        public abstract Location MapToLocation(Point point);

        /// <summary>
        /// Transforms a BoundingBox in geographic coordinates to a Rect in projected map coordinates.
        /// </summary>
        public virtual Rect BoundingBoxToRect(BoundingBox boundingBox)
        {
            return new Rect(
                LocationToMap(new Location(boundingBox.South, boundingBox.West)),
                LocationToMap(new Location(boundingBox.North, boundingBox.East)));
        }

        /// <summary>
        /// Transforms a Rect in projected map coordinates to a BoundingBox in geographic coordinates.
        /// Returns null when the Rect can not be transformed.
        /// </summary>
        public virtual BoundingBox RectToBoundingBox(Rect rect)
        {
            var sw = MapToLocation(new Point(rect.X, rect.Y));
            var ne = MapToLocation(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            return sw != null && ne != null
                ? new BoundingBox(sw.Latitude, sw.Longitude, ne.Latitude, ne.Longitude)
                : null;
        }

        /// <summary>
        /// Gets the CRS parameter value for a WMS GetMap request.
        /// </summary>
        public virtual string GetCrsValue()
        {
            return CrsId.StartsWith("AUTO:") || CrsId.StartsWith("AUTO2:")
                ? string.Format(CultureInfo.InvariantCulture, "{0},1,{1},{2}", CrsId, Center.Longitude, Center.Latitude)
                : CrsId;
        }

        /// <summary>
        /// Gets the BBOX parameter value for a WMS GetMap request.
        /// </summary>
        public virtual string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3}", rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height));
        }

        /// <summary>
        /// Checks if the X and Y values of a Point are neither NaN nor Infinity.
        /// </summary>
        public static bool IsValid(Point point)
        {
            return !double.IsNaN(point.X) && !double.IsInfinity(point.X) &&
                   !double.IsNaN(point.Y) && !double.IsInfinity(point.Y);
        }

        /// <summary>
        /// Checks if the X, Y, Width and Height values of a Rect are neither NaN nor Infinity.
        /// </summary>
        public static bool IsValid(Rect rect)
        {
            return !double.IsNaN(rect.X) && !double.IsInfinity(rect.X) &&
                   !double.IsNaN(rect.Y) && !double.IsInfinity(rect.Y) &&
                   !double.IsNaN(rect.Width) && !double.IsInfinity(rect.Width) &&
                   !double.IsNaN(rect.Height) && !double.IsInfinity(rect.Height);
        }
    }
}
