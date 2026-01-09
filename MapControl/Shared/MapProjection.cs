using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
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
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(MapProjectionConverter))]
#endif
    public abstract class MapProjection
    {
        public const double Wgs84EquatorialRadius = 6378137d;
        public const double Wgs84MeterPerDegree = Wgs84EquatorialRadius * Math.PI / 180d;
        public const double Wgs84Flattening = 1d / 298.257223563;

        // Arithmetic mean radius (2*a + b) / 3 == (1 - f/3) * a
        // https://en.wikipedia.org/wiki/Earth_radius#Arithmetic_mean_radius
        //
        public const double Wgs84MeanRadius = (1d - Wgs84Flattening / 3d) * Wgs84EquatorialRadius;

        public static MapProjectionFactory Factory
        {
            get => field ??= new MapProjectionFactory();
            set;
        }

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
        /// Gets the relative map scale at the specified geographic coordinates.
        /// </summary>
        public virtual Point RelativeScale(double latitude, double longitude) => new Point(1d, 1d);

        /// <summary>
        /// Transforms geographic coordinates to a Point in projected map coordinates.
        /// Returns null when the location can not be transformed.
        /// </summary>
        public abstract Point? LocationToMap(double latitude, double longitude);

        /// <summary>
        /// Transforms projected map coordinates to a Location in geographic coordinates.
        /// Returns null when the coordinates can not be transformed.
        /// </summary>
        public abstract Location MapToLocation(double x, double y);

        /// <summary>
        /// Gets the relative map scale at the specified geographic Location.
        /// </summary>
        public Point RelativeScale(Location location) => RelativeScale(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in projected map coordinates.
        /// Returns null when the Location can not be transformed.
        /// </summary>
        public Point? LocationToMap(Location location) => LocationToMap(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms a Point in projected map coordinates to a Location in geographic coordinates.
        /// Returns null when the Point can not be transformed.
        /// </summary>
        public Location MapToLocation(Point point) => MapToLocation(point.X, point.Y);

        /// <summary>
        /// Transforms a BoundingBox in geographic coordinates to a Rect in projected map coordinates.
        /// Returns null when the BoundingBox can not be transformed.
        /// </summary>
        public virtual Rect? BoundingBoxToMap(BoundingBox boundingBox)
        {
            var southWest = LocationToMap(boundingBox.South, boundingBox.West);
            var northEast = LocationToMap(boundingBox.North, boundingBox.East);

            return southWest.HasValue && northEast.HasValue ? new Rect(southWest.Value, northEast.Value) : null;
        }

        /// <summary>
        /// Transforms a Rect in projected map coordinates to a BoundingBox in geographic coordinates.
        /// Returns null when the MapRect can not be transformed.
        /// </summary>
        public virtual BoundingBox MapToBoundingBox(Rect rect)
        {
            var southWest = MapToLocation(rect.X, rect.Y);
            var northEast = MapToLocation(rect.X + rect.Width, rect.Y + rect.Height);

            return southWest != null && northEast != null ? new BoundingBox(southWest, northEast) : null;
        }

        /// <summary>
        /// Transforms a LatLonBox in geographic coordinates to a rotated Rect in projected map coordinates.
        /// Returns null when the LatLonBox can not be transformed.
        /// </summary>
        public virtual (Rect?, double) LatLonBoxToMap(LatLonBox latLonBox)
        {
            Rect? rect = null;
            var rotation = 0d;
            var centerLatitude = latLonBox.Center.Latitude;
            var centerLongitude = latLonBox.Center.Longitude;
            Point? center, north, south, west, east;

            if ((center = LocationToMap(centerLatitude, centerLongitude)).HasValue &&
                (north = LocationToMap(latLonBox.North, centerLongitude)).HasValue &&
                (south = LocationToMap(latLonBox.South, centerLongitude)).HasValue &&
                (west = LocationToMap(centerLatitude, latLonBox.West)).HasValue &&
                (east = LocationToMap(centerLatitude, latLonBox.East)).HasValue)
            {
                var dx1 = east.Value.X - west.Value.X;
                var dy1 = east.Value.Y - west.Value.Y;
                var dx2 = north.Value.X - south.Value.X;
                var dy2 = north.Value.Y - south.Value.Y;
                var width = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
                var height = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                var x = center.Value.X - width / 2d;
                var y = center.Value.Y - height / 2d;

                // Additional rotation caused by the projection, calculated as mean value
                // of the two angles measured relative to the east and north axis.
                //
                var r1 = (Math.Atan2(dy1, dx1) * 180d / Math.PI + 180d) % 360d - 180d;
                var r2 = (Math.Atan2(-dx2, dy2) * 180d / Math.PI + 180d) % 360d - 180d;

                rect = new Rect(x, y, width, height);
                rotation = latLonBox.Rotation + (r1 + r2) / 2d;
            }

            return (rect, rotation);
        }

        public override string ToString()
        {
            return CrsId;
        }

        /// <summary>
        /// Creates a MapProjection instance from a CRS id string.
        /// </summary>
        public static MapProjection Parse(string crsId)
        {
            return Factory.GetProjection(crsId);
        }
    }
}
