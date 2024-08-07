﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
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
        /// Initializes a ViewTransform from a map center point in projected coordinates,
        /// a view conter point, a scaling factor from projected coordinates to view coordinates
        /// and a rotation angle in degrees.
        /// </summary>
        public void SetTransform(Point mapCenter, Point viewCenter, double scale, double rotation)
        {
            Scale = scale;
            Rotation = ((rotation % 360d) + 360d) % 360d;

            MapToViewMatrix
                = Matrix.CreateTranslation(-mapCenter.X, -mapCenter.Y)
                * Matrix.CreateScale(scale, -scale)
                * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                * Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);

            ViewToMapMatrix = MapToViewMatrix.Invert();
        }

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

        /// <summary>
        /// Transform relative to absolute map scale. Returns horizontal and vertical
        /// scaling factors from meters to view coordinates.
        /// </summary>
        public Point GetMapScale(Point relativeScale)
        {
            return new Point(Scale * relativeScale.X, Scale * relativeScale.Y);
        }

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for a relative map scale.
        /// </summary>
        public Matrix GetMapTransform(Point relativeScale)
        {
            var scale = GetMapScale(relativeScale);

            return Matrix.CreateScale(scale.X, scale.Y)
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
            var viewOrigin = MapToView(mapOrigin);

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
            var origin = ViewToMap(new Point());

            var transformScale = tileMatrixScale / Scale;

            var transform
                = Matrix.CreateScale(transformScale, transformScale)
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
