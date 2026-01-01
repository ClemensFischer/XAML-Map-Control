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
#if AVALONIA
            MapToViewMatrix = Matrix.CreateTranslation(-mapCenter.X, -mapCenter.Y)
                            * Matrix.CreateScale(scale, -scale)
                            * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                            * Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);

            ViewToMapMatrix = MapToViewMatrix.Invert();
#else
            var transform = new Matrix(scale, 0d, 0d, -scale, -scale * mapCenter.X, scale * mapCenter.Y);
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
#if AVALONIA
            return Matrix.CreateScale(Scale * relativeScale.X, Scale * relativeScale.Y)
                 * Matrix.CreateRotation(Matrix.ToRadians(Rotation));
#else
            var transform = new Matrix(Scale * relativeScale.X, 0d, 0d, Scale * relativeScale.Y, 0d, 0d);
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
#if AVALONIA
            return Matrix.CreateScale(transformScale, transformScale)
                 * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                 * Matrix.CreateTranslation(viewOrigin.X, viewOrigin.Y);
#else
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);
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

            var transformScale = tileMatrixScale / Scale;
#if AVALONIA
            var transform = Matrix.CreateScale(transformScale, transformScale)
                          * Matrix.CreateRotation(Matrix.ToRadians(-Rotation))
                          * Matrix.CreateTranslation(
                                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y));

            // Transform view bounds to tile pixel bounds.
            //
            return new Rect(0d, 0d, viewWidth, viewHeight).TransformToAABB(transform);
#else
            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);
            transform.Rotate(-Rotation);

            // Translate origin to tile matrix origin in pixels.
            //
            transform.Translate(
                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y));

            // Transform view bounds to tile pixel bounds.
            //
            return new MatrixTransform { Matrix = transform }
                .TransformBounds(new Rect(0d, 0d, viewWidth, viewHeight));
#endif
        }
    }
}
