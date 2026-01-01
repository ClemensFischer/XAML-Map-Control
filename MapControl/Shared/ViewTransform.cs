#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
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
        /// Initializes a ViewTransform from a map center point in projected coordinates,
        /// a view conter point, a scaling factor from projected coordinates to view coordinates
        /// and a rotation angle in degrees.
        /// </summary>
        public void SetTransform(Point mapCenter, Point viewCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = ((rotation % 360d) + 360d) % 360d;

            var transform = new Matrix(scale, 0d, 0d, -scale, -scale * mapCenter.X, scale * mapCenter.Y);
#if AVALONIA
            MapToViewMatrix = transform
                * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                * Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);

            ViewToMapMatrix = MapToViewMatrix.Invert();
#else
            transform.Rotate(Rotation);
            transform.Translate(viewCenter.X, viewCenter.Y);
            MapToViewMatrix = transform;

            transform.Invert();
            ViewToMapMatrix = transform;
#endif
        }

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for a relative map scale.
        /// </summary>
        public Matrix GetMapTransform(Point relativeScale)
        {
            var transform = new Matrix(Scale * relativeScale.X, 0d, 0d, Scale * relativeScale.Y, 0d, 0d);
#if AVALONIA
            return transform * Matrix.CreateRotation(Matrix.ToRadians(Rotation));
#else
            transform.Rotate(Rotation);
            return transform;
#endif
        }

        /// <summary>
        /// Gets the transform Matrix for the RenderTranform of a MapTileLayer.
        /// </summary>
        public Matrix GetTileLayerTransform(double tileMatrixScale, Point tileMatrixTopLeft, Point tileMatrixOrigin)
        {
            // Tile matrix origin in map coordinates.
            //
            var mapOrigin = new Point(
                tileMatrixTopLeft.X + tileMatrixOrigin.X / tileMatrixScale,
                tileMatrixTopLeft.Y - tileMatrixOrigin.Y / tileMatrixScale);

            // Tile matrix origin in view coordinates.
            //
            var viewOrigin = MapToViewMatrix.Transform(mapOrigin);

            var transformScale = Scale / tileMatrixScale;
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);
#if AVALONIA
            return transform
                * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                * Matrix.CreateTranslation(viewOrigin.X, viewOrigin.Y);
#else
            transform.Rotate(Rotation);
            transform.Translate(viewOrigin.X, viewOrigin.Y);
            return transform;
#endif
        }

        /// <summary>
        /// Gets the pixel bounds of a tile matrix.
        /// </summary>
        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, double viewWidth, double viewHeight)
        {
            // View origin in map coordinates.
            //
            var origin = ViewToMapMatrix.Transform(new Point());

            // Translation from origin to tile matrix origin in pixels.
            //
            var originOffsetX = tileMatrixScale * (origin.X - tileMatrixTopLeft.X);
            var originOffsetY = tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y);

            var transformScale = tileMatrixScale / Scale;
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);
            var viewRect = new Rect(0d, 0d, viewWidth, viewHeight);
#if AVALONIA
            return viewRect.TransformToAABB(transform
                * Matrix.CreateRotation(Matrix.ToRadians(-Rotation))
                * Matrix.CreateTranslation(originOffsetX, originOffsetY));
#else
            transform.Rotate(-Rotation);
            transform.Translate(originOffsetX, originOffsetY);

            // Transform view bounds to tile pixel bounds.
            //
            return new MatrixTransform { Matrix = transform }.TransformBounds(viewRect);
#endif
        }
    }
}
