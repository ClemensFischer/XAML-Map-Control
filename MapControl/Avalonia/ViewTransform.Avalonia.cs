// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// Defines the transformation between projected map coordinates in meters
    /// and view coordinates in pixels.
    /// </summary>
    public class ViewTransform
    {
        public static double ZoomLevelToScale(double zoomLevel)
        {
            return 256d * Math.Pow(2d, zoomLevel) / (360d * MapProjection.Wgs84MeterPerDegree);
        }

        public static double ScaleToZoomLevel(double scale)
        {
            return Math.Log(scale * 360d * MapProjection.Wgs84MeterPerDegree / 256d, 2d);
        }

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
        /// Transforms a Point from projected map coordinates to view coordinates.
        /// </summary>
        public Point MapToView(Point point)
        {
            return MapToViewMatrix.Transform(point);
        }

        /// <summary>
        /// Transforms a Point from view coordinates to projected map coordinates.
        /// </summary>
        public Point ViewToMap(Point point)
        {
            return ViewToMapMatrix.Transform(point);
        }

        public void SetTransform(Point mapCenter, Point viewCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = ((rotation % 360d) + 360d) % 360d;

            MapToViewMatrix = new Matrix(Scale, 0d, 0d, -Scale, -Scale * mapCenter.X, Scale * mapCenter.Y)
                .Append(Matrix.CreateRotation(Rotation * Math.PI / 180d))
                .Append(Matrix.CreateTranslation(viewCenter.X, viewCenter.Y));

            ViewToMapMatrix = MapToViewMatrix.Invert();
        }

        public Matrix GetTileLayerTransform(double tileMatrixScale, Point tileMatrixTopLeft, Point tileMatrixOrigin)
        {
            // Tile matrix origin in map coordinates.
            //
            var mapOrigin = new Point(
                tileMatrixTopLeft.X + tileMatrixOrigin.X / tileMatrixScale,
                tileMatrixTopLeft.Y - tileMatrixOrigin.Y / tileMatrixScale);

            // Tile matrix origin in view coordinates.
            //
            var viewOrigin = MapToView(mapOrigin);

            var transformScale = Scale / tileMatrixScale;

            return new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d)
                .Append(Matrix.CreateRotation(Rotation * Math.PI / 180d))
                .Append(Matrix.CreateTranslation(viewOrigin.X, viewOrigin.Y));
        }

        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, Size viewSize)
        {
            // View origin in map coordinates.
            //
            var origin = ViewToMap(new Point());

            var transformScale = tileMatrixScale / Scale;

            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d)
                .Append(Matrix.CreateRotation(-Rotation * Math.PI / 180d));

            // Translate origin to tile matrix origin in pixels.
            //
            transform = transform.Append(Matrix.CreateTranslation(
                tileMatrixScale * (origin.X - tileMatrixTopLeft.X),
                tileMatrixScale * (tileMatrixTopLeft.Y - origin.Y)));

            // Transform view bounds to tile pixel bounds.
            //
            return new Rect(0d, 0d, viewSize.Width, viewSize.Height).TransformToAABB(transform);
        }
    }
}
