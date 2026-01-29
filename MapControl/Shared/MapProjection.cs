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

        /// <summary>
        /// Gets the WMS 1.3.0 CRS identifier.
        /// </summary>
        public string CrsId { get; protected set; } = "";

        /// <summary>
        /// Indicates whether the projection is normal cylindrical, see
        /// https://en.wikipedia.org/wiki/Map_projection#Normal_cylindrical.
        /// </summary>
        public bool IsNormalCylindrical { get; protected set; }

        /// <summary>
        /// The earth ellipsoid semi-major axis, or spherical earth radius respectively, in meters.
        /// </summary>
        public double EquatorialRadius { get; set; } = Wgs84EquatorialRadius;

        public double MeterPerDegree => EquatorialRadius * Math.PI / 180d;

        /// <summary>
        /// Gets the relative transform at the specified geographic coordinates.
        /// The returned Matrix represents the local distortion of the map projection.
        /// </summary>
        public abstract Matrix RelativeTransform(double latitude, double longitude);

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
        /// with an optional transform Matrix.
        /// </summary>
        public (Rect, Matrix?) BoundingBoxToMap(BoundingBox boundingBox)
        {
            Rect rect;
            Matrix? transform = null;

            if (IsNormalCylindrical)
            {
                var southWest = LocationToMap(boundingBox.South, boundingBox.West);
                var northEast = LocationToMap(boundingBox.North, boundingBox.East);

                rect = new Rect(southWest.X, southWest.Y, northEast.X - southWest.X, northEast.Y - southWest.Y);
            }
            else
            {
                var latitude = (boundingBox.South + boundingBox.North) / 2d;
                var longitude = (boundingBox.West + boundingBox.East) / 2d;
                var center = LocationToMap(latitude, longitude);
                var width = MeterPerDegree * (boundingBox.East - boundingBox.West) * Math.Cos(latitude * Math.PI / 180d);
                var height = MeterPerDegree * (boundingBox.North - boundingBox.South);

                rect = new Rect(center.X - width / 2d, center.Y - height / 2d, width, height);
                transform = RelativeTransform(latitude, longitude);
            }

            return (rect, transform);
        }

        /// <summary>
        /// Transforms a Rect in projected map coordinates to a BoundingBox in geographic coordinates.
        /// </summary>
        public BoundingBox MapToBoundingBox(Rect rect)
        {
            var southWest = MapToLocation(rect.X, rect.Y);
            var northEast = MapToLocation(rect.X + rect.Width, rect.Y + rect.Height);

            return new BoundingBox(southWest.Latitude, southWest.Longitude, northEast.Latitude, northEast.Longitude);
        }

        public override string ToString()
        {
            return CrsId;
        }
    }
}
