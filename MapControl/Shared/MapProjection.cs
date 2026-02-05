using System;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Implements a map projection, a transformation between geographic coordinates,
    /// i.e. latitude and longitude in degrees, and cartesian map coordinates in meters.
    /// See https://en.wikipedia.org/wiki/Map_projection.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(MapProjectionConverter))]
#endif
    public abstract class MapProjection
    {
        public const double Wgs84EquatorialRadius = 6378137d;
        public const double Wgs84Flattening = 1d / 298.257223563;
        public const double Wgs84MeterPerDegree = Wgs84EquatorialRadius * Math.PI / 180d;

        public static MapProjectionFactory Factory
        {
            get => field ??= new MapProjectionFactory();
            set;
        }

        /// <summary>
        /// Creates a MapProjection instance from a CRS identifier string.
        /// </summary>
        public static MapProjection Parse(string crsId)
        {
            return Factory.GetProjection(crsId);
        }

        public override string ToString() => CrsId;

        /// <summary>
        /// Gets the WMS 1.3.0 CRS identifier.
        /// </summary>
        public string CrsId { get; protected set; }

        public double EquatorialRadius { get; protected set; } = Wgs84EquatorialRadius;
        public double Flattening { get; protected set; } = Wgs84Flattening;
        public double ScaleFactor { get; protected set; } = 1d;
        public double CentralMeridian { get; protected set; }
        public double LatitudeOfOrigin { get; protected set; }
        public double FalseEasting { get; protected set; }
        public double FalseNorthing { get; protected set; }
        public bool IsNormalCylindrical { get; protected set; }

        /// <summary>
        /// Gets the grid convergence angle in degrees at the specified geographic coordinates.
        /// Used for rotating the Rect resulting from BoundingBoxToMap in non-normal-cylindrical
        /// projections, i.e. Transverse Mercator and Polar Stereographic.
        /// </summary>
        public virtual double GridConvergence(double latitude, double longitude) => 0d;

        /// <summary>
        /// Gets the relative transform at the specified geographic coordinates.
        /// The returned Matrix represents the local relative scale and rotation.
        /// </summary>
        public virtual Matrix RelativeTransform(double latitude, double longitude)
        {
            var transform = new Matrix(ScaleFactor, 0d, 0d, ScaleFactor, 0d, 0d);
            transform.Rotate(-GridConvergence(latitude, longitude));
            return transform;
        }

        /// <summary>
        /// Transforms geographic coordinates to a Point in projected map coordinates.
        /// </summary>
        public abstract Point LocationToMap(double latitude, double longitude);

        /// <summary>
        /// Transforms projected map coordinates to a Location in geographic coordinates.
        /// </summary>
        public abstract Location MapToLocation(double x, double y);

        /// <summary>
        /// Gets the relative transform at the specified geographic Location.
        /// </summary>
        public Matrix RelativeTransform(Location location) => RelativeTransform(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in projected map coordinates.
        /// </summary>
        public Point LocationToMap(Location location) => LocationToMap(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms a Point in projected map coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location MapToLocation(Point point) => MapToLocation(point.X, point.Y);

        /// <summary>
        /// Transforms a BoundingBox in geographic coordinates to a Rect in projected map coordinates
        /// with an optional rotation angle in degrees for non-normal-cylindrical projections.
        /// </summary>
        public (Rect, double) BoundingBoxToMap(BoundingBox boundingBox)
        {
            Rect rect;
            var rotation = 0d;
            var southWest = LocationToMap(boundingBox.South, boundingBox.West);
            var northEast = LocationToMap(boundingBox.North, boundingBox.East);

            if (IsNormalCylindrical)
            {
                rect = new Rect(southWest.X, southWest.Y, northEast.X - southWest.X, northEast.Y - southWest.Y);
            }
            else
            {
                var southEast = LocationToMap(boundingBox.South, boundingBox.East);
                var northWest = LocationToMap(boundingBox.North, boundingBox.West);
                var west = new Point((southWest.X + northWest.X) / 2d, (southWest.Y + northWest.Y) / 2d);
                var east = new Point((southEast.X + northEast.X) / 2d, (southEast.Y + northEast.Y) / 2d);
                var south = new Point((southWest.X + southEast.X) / 2d, (southWest.Y + southEast.Y) / 2d);
                var north = new Point((northWest.X + northEast.X) / 2d, (northWest.Y + northEast.Y) / 2d);
                var dxWidth = east.X - west.X;
                var dyWidth = east.Y - west.Y;
                var dxHeight = north.X - south.X;
                var dyHeight = north.Y - south.Y;
                var width = Math.Sqrt(dxWidth * dxWidth + dyWidth * dyWidth);
                var height = Math.Sqrt(dxHeight * dxHeight + dyHeight * dyHeight);
                var x = (south.X + north.X - width) / 2d;  // cx-w/2
                var y = (south.Y + north.Y - height) / 2d; // cy-h/2

                rect = new Rect(x, y, width, height);
                rotation = Math.Atan2(-dxHeight, dyHeight) * 180d / Math.PI;
            }

            return (rect, rotation);
        }
    }
}
