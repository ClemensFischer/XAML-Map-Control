// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
    /// Defines the transformation between cartesian map coordinates and viewport coordinates.
    /// </summary>
    public class ViewTransform
    {
        /// <summary>
        /// Gets the transform matrix from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public Matrix MapToViewMatrix { get; private set; }

        /// <summary>
        /// Gets the transform matrix from viewport coordinates to cartesian map coordinates.
        /// </summary>
        public Matrix ViewToMapMatrix { get; private set; }

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public double Scale { get; private set; }

        /// <summary>
        /// Gets the rotation angle of the transform matrix.
        /// </summary>
        public double Rotation { get; private set; }

        /// <summary>
        /// Transforms a Point from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public Point MapToView(Point point)
        {
            return MapToViewMatrix.Transform(point);
        }

        /// <summary>
        /// Transforms a Point from viewport coordinates to cartesian map coordinates.
        /// </summary>
        public Point ViewToMap(Point point)
        {
            return ViewToMapMatrix.Transform(point);
        }

        public void SetTransform(Point mapCenter, Point viewportCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = rotation;

            var transform = new Matrix(Scale, 0d, 0d, -Scale, -Scale * mapCenter.X, Scale * mapCenter.Y);

            transform.Rotate(Rotation);
            transform.Translate(viewportCenter.X, viewportCenter.Y);

            MapToViewMatrix = transform;

            transform.Invert();

            ViewToMapMatrix = transform;
        }

        public Matrix GetTileLayerTransform(double tileMatrixScale, Point tileMatrixTopLeft, Point tileMatrixOrigin)
        {
            var transformScale = Scale / tileMatrixScale;
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);

            transform.Rotate(Rotation);

            // tile matrix origin in map coordinates
            //
            var mapOrigin = new Point(
                tileMatrixTopLeft.X + tileMatrixOrigin.X / tileMatrixScale,
                tileMatrixTopLeft.Y - tileMatrixOrigin.Y / tileMatrixScale);

            // tile matrix origin in viewport coordinates
            //
            var viewOrigin = MapToView(mapOrigin);

            transform.Translate(viewOrigin.X, viewOrigin.Y);

            return transform;
        }

        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, Size viewportSize)
        {
            var transformScale = tileMatrixScale / Scale;
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);

            transform.Rotate(-Rotation);

            // viewport origin in map coordinates
            //
            var origin = ViewToMap(new Point());

            // translate origin to tile matrix origin in pixels
            //
            transform.Translate(
                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y));

            // transform viewport bounds to tile pixel bounds
            //
            return new MatrixTransform { Matrix = transform }
                .TransformBounds(new Rect(0d, 0d, viewportSize.Width, viewportSize.Height));
        }
    }
}
