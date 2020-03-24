// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Defines a map projection between geographic coordinates, cartesian map coordinates and viewport coordinates.
    /// </summary>
    public abstract class MapProjection
    {
        public const double Wgs84EquatorialRadius = 6378137d;
        public const double Wgs84Flattening = 1d / 298.257223563;
        public static readonly double Wgs84Eccentricity = Math.Sqrt((2d - Wgs84Flattening) * Wgs84Flattening);

        public const double Wgs84MetersPerDegree = Wgs84EquatorialRadius * Math.PI / 180d;

        /// <summary>
        /// Gets or sets the WMS 1.3.0 CRS identifier.
        /// </summary>
        public string CrsId { get; set; }

        /// <summary>
        /// Indicates if this is a normal cylindrical projection.
        /// </summary>
        public virtual bool IsNormalCylindrical
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates if this is a web mercator projection, i.e. compatible with MapTileLayer.
        /// </summary>
        public virtual bool IsWebMercator
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the absolute value of the minimum and maximum latitude that can be transformed.
        /// </summary>
        public virtual double MaxLatitude
        {
            get { return 90d; }
        }

        /// <summary>
        /// Gets the scale factor from geographic to cartesian coordinates, on the line of true scale of a
        /// cylindrical projection (usually the equator), or at the projection center of an azimuthal projection.
        /// </summary>
        public virtual double TrueScale
        {
            get { return Wgs84MetersPerDegree; }
        }

        /// <summary>
        /// Gets the projection center. Only relevant for azimuthal projections.
        /// </summary>
        public Location ProjectionCenter { get; private set; } = new Location();

        /// <summary>
        /// Gets the transform matrix from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public Matrix ViewportTransform { get; private set; }

        /// <summary>
        /// Gets the transform matrix from viewport coordinates to cartesian map coordinates.
        /// </summary>
        public Matrix InverseViewportTransform { get; private set; }

        /// <summary>
        /// Gets the rotation angle of the ViewportTransform matrix.
        /// </summary>
        public double ViewportRotation { get; private set; }

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates
        /// at the projection's point of true scale.
        /// </summary>
        public double ViewportScale { get; private set; }

        /// <summary>
        /// Gets the map scale at the specified Location as viewport coordinate units per meter (px/m).
        /// </summary>
        public virtual Vector GetMapScale(Location location)
        {
            return new Vector(ViewportScale, ViewportScale);
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in cartesian map coordinates.
        /// </summary>
        public abstract Point LocationToPoint(Location location);

        /// <summary>
        /// Transforms a Point in cartesian map coordinates to a Location in geographic coordinates.
        /// </summary>
        public abstract Location PointToLocation(Point point);

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
            return PointToLocation(InverseViewportTransform.Transform(point));
        }

        /// <summary>
        /// Transforms a Rect in viewport coordinates to a BoundingBox in geographic coordinates.
        /// </summary>
        public BoundingBox ViewportRectToBoundingBox(Rect rect)
        {
            var p1 = InverseViewportTransform.Transform(new Point(rect.X, rect.Y));
            var p2 = InverseViewportTransform.Transform(new Point(rect.X, rect.Y + rect.Height));
            var p3 = InverseViewportTransform.Transform(new Point(rect.X + rect.Width, rect.Y));
            var p4 = InverseViewportTransform.Transform(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            rect.X = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            rect.Y = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            rect.Width = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X))) - rect.X;
            rect.Height = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y))) - rect.Y;

            return RectToBoundingBox(rect);
        }

        /// <summary>
        /// Gets the CRS parameter value for a WMS GetMap request.
        /// </summary>
        public virtual string GetCrsValue()
        {
            return CrsId.StartsWith("AUTO:") || CrsId.StartsWith("AUTO2:")
                ? string.Format(CultureInfo.InvariantCulture, "{0},1,{1},{2}", CrsId, ProjectionCenter.Longitude, ProjectionCenter.Latitude)
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
        /// Sets ProjectionCenter, ViewportScale, ViewportRotation, ViewportTransform and InverseViewportTransform.
        /// </summary>
        public void SetViewportTransform(Location projectionCenter, Location mapCenter, Point viewportCenter, double zoomLevel, double rotation)
        {
            ProjectionCenter = projectionCenter;
            ViewportScale = 256d * Math.Pow(2d, zoomLevel) / (360d * TrueScale);
            ViewportRotation = rotation;

            var center = LocationToPoint(mapCenter);
            var matrix = CreateViewportTransform(center, viewportCenter);

            ViewportTransform = matrix;
            matrix.Invert();
            InverseViewportTransform = matrix;
        }

        private Matrix CreateViewportTransform(Point mapCenter, Point viewportCenter)
        {
            var matrix = new Matrix(ViewportScale, 0d, 0d, -ViewportScale, -ViewportScale * mapCenter.X, ViewportScale * mapCenter.Y);

            matrix.Rotate(ViewportRotation);
            matrix.Translate(viewportCenter.X, viewportCenter.Y);

            return matrix;
        }

        internal Matrix CreateTileLayerTransform(double tileGridScale, Point tileGridTopLeft, Point tileGridOrigin)
        {
            var scale = ViewportScale / tileGridScale;
            var matrix = new Matrix(scale, 0d, 0d, scale, 0d, 0d);

            matrix.Rotate(ViewportRotation);

            // tile grid origin in map coordinates
            //
            var mapOrigin = new Point(
                tileGridTopLeft.X + tileGridOrigin.X / tileGridScale,
                tileGridTopLeft.Y - tileGridOrigin.Y / tileGridScale);

            // tile grid origin in viewport coordinates
            //
            var viewOrigin = ViewportTransform.Transform(mapOrigin);

            matrix.Translate(viewOrigin.X, viewOrigin.Y);

            return matrix;
        }

        internal Rect GetTileBounds(double tileGridScale, Point tileGridTopLeft, Size viewportSize)
        {
            var scale = tileGridScale / ViewportScale;
            var matrix = new Matrix(scale, 0d, 0d, scale, 0d, 0d);

            matrix.Rotate(-ViewportRotation);

            // viewport origin in map coordinates
            //
            var origin = InverseViewportTransform.Transform(new Point());

            // translate origin to tile grid origin in pixels
            //
            matrix.Translate(
                tileGridScale * (origin.X - tileGridTopLeft.X),
                tileGridScale * (tileGridTopLeft.Y - origin.Y));

            // transforms viewport bounds to tile pixel bounds
            //
            var transform = new MatrixTransform { Matrix = matrix };

            return transform.TransformBounds(new Rect(0d, 0d, viewportSize.Width, viewportSize.Height));
        }
    }
}
