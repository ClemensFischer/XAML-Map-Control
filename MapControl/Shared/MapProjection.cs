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
        public static readonly double Wgs84Eccentricity = Math.Sqrt((2d - Wgs84Flattening) * Wgs84Flattening);

        private static MapProjectionFactory factory;

        public static MapProjectionFactory Factory
        {
            get => factory ??= new MapProjectionFactory();
            set => factory = value;
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

            var southWest = LocationToMap(new Location(boundingBox.South, boundingBox.West));
            var northEast = LocationToMap(new Location(boundingBox.North, boundingBox.East));

            if (southWest.HasValue && northEast.HasValue)
            {
                rect = new Rect(southWest.Value, northEast.Value);
            }

            return rect;
        }

        /// <summary>
        /// Transforms a Rect in projected map coordinates to a BoundingBox in geographic coordinates.
        /// Returns null when the MapRect can not be transformed.
        /// </summary>
        public virtual BoundingBox MapToBoundingBox(Rect rect)
        {
            BoundingBox boundingBox = null;
            var southWest = MapToLocation(new Point(rect.X, rect.Y));
            var northEast = MapToLocation(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            if (southWest != null && northEast != null)
            {
                boundingBox = new BoundingBox(southWest, northEast);
            }

            return boundingBox;
        }

        /// <summary>
        /// Transforms a LatLonBox in geographic coordinates to a rotated Rect in projected map coordinates.
        /// Returns null when the LatLonBox can not be transformed.
        /// </summary>
        public virtual Tuple<Rect, double> LatLonBoxToMap(LatLonBox latLonBox)
        {
            Tuple<Rect, double> rotatedRect = null;
            Point? center, north, south, west, east;
            var boxCenter = latLonBox.Center;

            if ((center = LocationToMap(boxCenter)).HasValue &&
                (north = LocationToMap(new Location(latLonBox.North, boxCenter.Longitude))).HasValue &&
                (south = LocationToMap(new Location(latLonBox.South, boxCenter.Longitude))).HasValue &&
                (west = LocationToMap(new Location(boxCenter.Latitude, latLonBox.West))).HasValue &&
                (east = LocationToMap(new Location(boxCenter.Latitude, latLonBox.East))).HasValue)
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

                rotatedRect = new Tuple<Rect, double>(new Rect(x, y, width, height), latLonBox.Rotation + (r1 + r2) / 2d);
            }

            return rotatedRect;
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
