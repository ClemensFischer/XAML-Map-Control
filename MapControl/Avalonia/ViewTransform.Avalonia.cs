using Avalonia;

namespace MapControl
{
    public partial class ViewTransform
    {
        /// <summary>
        /// Initializes a ViewTransform from a map center point in projected coordinates,
        /// a view conter point, a scaling factor from projected coordinates to view coordinates
        /// and a rotation angle in degrees.
        /// </summary>
        public void SetTransform(Point mapCenter, Point viewCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = ((rotation % 360d) + 360d) % 360d;

            MapToViewMatrix = Matrix.CreateTranslation(-mapCenter.X, -mapCenter.Y)
                            * Matrix.CreateScale(scale, -scale)
                            * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                            * Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);

            ViewToMapMatrix = MapToViewMatrix.Invert();
        }

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for a relative map scale.
        /// </summary>
        public Matrix GetMapTransform(Point relativeScale)
        {
            return Matrix.CreateScale(Scale * relativeScale.X, Scale * relativeScale.Y)
                 * Matrix.CreateRotation(Matrix.ToRadians(Rotation));
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

            return Matrix.CreateScale(transformScale, transformScale)
                 * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                 * Matrix.CreateTranslation(viewOrigin.X, viewOrigin.Y);
        }

        /// <summary>
        /// Gets the index bounds of a tile matrix.
        /// </summary>
        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, double viewWidth, double viewHeight)
        {
            // View origin in map coordinates.
            //
            var origin = ViewToMapMatrix.Transform(new Point());

            var transformScale = tileMatrixScale / Scale;

            var transform = Matrix.CreateScale(transformScale, transformScale)
                          * Matrix.CreateRotation(Matrix.ToRadians(-Rotation));

            // Translate origin to tile matrix origin in pixels.
            //
            transform *= Matrix.CreateTranslation(
                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y));

            // Transform view bounds to tile pixel bounds.
            //
            return new Rect(0d, 0d, viewWidth, viewHeight).TransformToAABB(transform);
        }

        internal static Matrix CreateTransformMatrix(
            double translation1X, double translation1Y,
            double rotation,
            double translation2X, double translation2Y)
        {
            return Matrix.CreateTranslation(translation1X, translation1Y)
                 * Matrix.CreateRotation(Matrix.ToRadians(rotation))
                 * Matrix.CreateTranslation(translation2X, translation2Y);
        }
    }
}
