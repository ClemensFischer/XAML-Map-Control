// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Defines a map projection between geographic coordinates and cartesian map coordinates 
    /// and viewport coordinates, i.e. pixels.
    /// </summary>
    public abstract partial class MapProjection
    {
        public const double Wgs84EquatorialRadius = 6378137d;
        public const double Wgs84Flattening = 1d / 298.257223563;

        public const double MetersPerDegree = Wgs84EquatorialRadius * Math.PI / 180d;

        /// <summary>
        /// Gets or sets the WMS 1.3.0 CRS Identifier.
        /// </summary>
        public abstract string CrsId { get; set; }

        /// <summary>
        /// Indicates if this is a web mercator projection, i.e. compatible with map tile layers.
        /// </summary>
        public virtual bool IsWebMercator { get; } = false;

        /// <summary>
        /// Indicates if this is an azimuthal projection.
        /// </summary>
        public virtual bool IsAzimuthal { get; } = false;

        /// <summary>
        /// Gets the scale factor from longitude to x values of a normal cylindrical projection.
        /// Returns NaN if this is not a normal cylindrical projection.
        /// </summary>
        public virtual double LongitudeScale { get; } = 1d;

        /// <summary>
        /// Gets the absolute value of the minimum and maximum latitude that can be transformed.
        /// </summary>
        public virtual double MaxLatitude { get; } = 90d;

        /// <summary>
        /// Gets the transformation from cartesian map coordinates to viewport coordinates (pixels).
        /// </summary>
        public MatrixTransform ViewportTransform { get; } = new MatrixTransform();

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public double ViewportScale { get; protected set; }

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates for the specified zoom level.
        /// </summary>
        public virtual double GetViewportScale(double zoomLevel)
        {
            return Math.Pow(2d, zoomLevel) * TileSource.TileSize / 360d;
        }

        /// <summary>
        /// Gets the map scale at the specified Location as viewport coordinate units per meter (px/m).
        /// </summary>
        public abstract Point GetMapScale(Location location);

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in cartesian map coordinates.
        /// </summary>
        public abstract Point LocationToPoint(Location location);

        /// <summary>
        /// Transforms a Point in cartesian map coordinates to a Location in geographic coordinates.
        /// </summary>
        public abstract Location PointToLocation(Point point);

        /// <summary>
        /// Translates a Location in geographic coordinates by the specified small amount in viewport coordinates.
        /// </summary>
        public abstract Location TranslateLocation(Location location, Point translation);

        /// <summary>
        /// Transforms a BoundingBox in geographic coordinates to a Rect in cartesian map coordinates.
        /// </summary>
        public virtual Rect BoundingBoxToRect(BoundingBox boundingBox)
        {
            return new Rect(
                LocationToPoint(new Location(boundingBox.South, boundingBox.West)),
                LocationToPoint(new Location(boundingBox.North, boundingBox.East)));
        }

        /// <summary>
        /// Transforms a Rect in cartesian map coordinates to a BoundingBox in geographic coordinates.
        /// </summary>
        public virtual BoundingBox RectToBoundingBox(Rect rect)
        {
            var sw = PointToLocation(new Point(rect.X, rect.Y));
            var ne = PointToLocation(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            return new BoundingBox(sw.Latitude, sw.Longitude, ne.Latitude, ne.Longitude);
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in viewport coordinates.
        /// </summary>
        public Point LocationToViewportPoint(Location location)
        {
            return ViewportTransform.Transform(LocationToPoint(location));
        }

        /// <summary>
        /// Transforms a Point in viewport coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewportPointToLocation(Point point)
        {
            return PointToLocation(ViewportTransform.Inverse.Transform(point));
        }

        /// <summary>
        /// Transforms a Rect in viewport coordinates to a BoundingBox in geographic coordinates.
        /// </summary>
        public BoundingBox ViewportRectToBoundingBox(Rect rect)
        {
            return RectToBoundingBox(ViewportTransform.Inverse.TransformBounds(rect));
        }

        /// <summary>
        /// Sets MapCenter, ViewportCenter, ViewportScale and ViewportTransform values.
        /// </summary>
        public virtual void SetViewportTransform(Location center, Point viewportCenter, double zoomLevel, double heading)
        {
            ViewportScale = GetViewportScale(zoomLevel);

            ViewportTransform.Matrix = MatrixEx.TranslateScaleRotateTranslate(
                LocationToPoint(center), ViewportScale, -ViewportScale, heading, viewportCenter);
        }

        /// <summary>
        /// Gets a WMS 1.3.0 query parameter string from the specified bounding box,
        /// e.g. "CRS=...&BBOX=...&WIDTH=...&HEIGHT=..."
        /// </summary>
        public virtual string WmsQueryParameters(BoundingBox boundingBox, string version = "1.3.0")
        {
            var format = "CRS={0}&BBOX={1},{2},{3},{4}&WIDTH={5}&HEIGHT={6}";

            if (version.StartsWith("1.1."))
            {
                format = "SRS={0}&BBOX={1},{2},{3},{4}&WIDTH={5}&HEIGHT={6}";
            }
            else if (CrsId == "EPSG:4326")
            {
                format = "CRS={0}&BBOX={2},{1},{4},{3}&WIDTH={5}&HEIGHT={6}";
            }

            var rect = BoundingBoxToRect(boundingBox);
            var width = (int)Math.Round(ViewportScale * rect.Width);
            var height = (int)Math.Round(ViewportScale * rect.Height);

            return string.Format(CultureInfo.InvariantCulture, format, CrsId,
                rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height), width, height);
        }
    }
}
