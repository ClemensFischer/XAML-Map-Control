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
    /// Defines the transformation between projected map coordinates in meters
    /// and view coordinates in pixels.
    /// </summary>
    public class ViewTransform
    {
        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double Scale { get; private set; }

        /// <summary>
        /// Gets the rotation angle of the transform matrix.
        /// </summary>
        public double Rotation { get; private set; }

        /// <summary>
        /// Gets the transform matrix from projected map coordinates to view coordinates.
        /// </summary>
        public Matrix MapToViewMatrix { get; private set; }

        /// <summary>
        /// Gets the transform matrix from view coordinates to projected map coordinates.
        /// </summary>
        public Matrix ViewToMapMatrix { get; private set; }

        /// <summary>
        /// Transforms a Point in projected map coordinates to a Point in view coordinates.
        /// </summary>
        public Point MapToView(Point point) => MapToViewMatrix.Transform(point);

        /// <summary>
        /// Transforms a Point in view coordinates to a Point in projected map coordinates.
        /// </summary>
        public Point ViewToMap(Point point) => ViewToMapMatrix.Transform(point);

        /// <summary>
        /// Gets an axis-aligned bounding box in projected map coordinates that contains
        /// a rectangle in view coordinates.
        /// </summary>
        public Rect ViewToMapBounds(Rect rect) => TransformBounds(ViewToMapMatrix, rect);

        /// <summary>
        /// Initializes a ViewTransform from a map center point in projected coordinates,
        /// a view conter point, a scaling factor from projected coordinates to view coordinates
        /// and a rotation angle in degrees.
        /// </summary>
        public void SetTransform(Point mapCenter, Point viewCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = ((rotation % 360d) + 360d) % 360d;

            var transform = new Matrix(scale, 0d, 0d, -scale, -scale * mapCenter.X, scale * mapCenter.Y);
            transform.Rotate(Rotation);
            transform.Translate(viewCenter.X, viewCenter.Y);
            MapToViewMatrix = transform;

            transform.Invert();
            ViewToMapMatrix = transform;
        }

        /// <summary>
        /// Gets the transform Matrix for the RenderTranform of a MapTileLayer or WmtsTileMatrixLayer.
        /// </summary>
        public Matrix GetTileLayerTransform(double tileMatrixScale, Point tileMatrixTopLeft, Point tileMatrixOrigin)
        {
            var scale = Scale / tileMatrixScale;
            var transform = new Matrix(scale, 0d, 0d, scale, 0d, 0d);
            transform.Rotate(Rotation);

            // Tile matrix origin in map coordinates.
            //
            var mapOrigin = new Point(
                tileMatrixTopLeft.X + tileMatrixOrigin.X / tileMatrixScale,
                tileMatrixTopLeft.Y - tileMatrixOrigin.Y / tileMatrixScale);

            // Tile matrix origin in view coordinates.
            //
            var viewOrigin = MapToViewMatrix.Transform(mapOrigin);
            transform.Translate(viewOrigin.X, viewOrigin.Y);

            return transform;
        }

        /// <summary>
        /// Gets the pixel bounds of a tile matrix.
        /// </summary>
        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, double viewWidth, double viewHeight)
        {
            var scale = tileMatrixScale / Scale;
            var transform = new Matrix(scale, 0d, 0d, scale, 0d, 0d);
            transform.Rotate(-Rotation);

            // View origin in map coordinates.
            //
            var origin = ViewToMapMatrix.Transform(new Point());

            // Translation from origin to tile matrix origin in pixels.
            //
            transform.Translate(
                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y));

            // Transform view bounds to tile pixel bounds.
            //
            return TransformBounds(transform, new Rect(0d, 0d, viewWidth, viewHeight));
        }

        private static Rect TransformBounds(Matrix transform, Rect rect)
        {
            if (transform.M12 == 0d && transform.M21 == 0d)
            {
                return new Rect(
                    rect.X * transform.M11 + transform.OffsetX,
                    rect.Y * transform.M22 + transform.OffsetY,
                    rect.Width * transform.M11,
                    rect.Height * transform.M22);
            }

            var p1 = transform.Transform(new Point(rect.X, rect.Y));
            var p2 = transform.Transform(new Point(rect.X, rect.Y + rect.Height));
            var p3 = transform.Transform(new Point(rect.X + rect.Width, rect.Y));
            var p4 = transform.Transform(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            var x1 = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            var y1 = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            var x2 = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X)));
            var y2 = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y)));

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}
