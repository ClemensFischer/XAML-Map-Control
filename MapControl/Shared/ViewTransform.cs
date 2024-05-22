// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

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

            var transform = new Matrix(scale, 0d, 0d, -scale, -scale * mapCenter.X, scale * mapCenter.Y);
            transform.Rotate(Rotation);
            transform.Translate(viewCenter.X, viewCenter.Y);

            MapToViewMatrix = transform;

            transform.Invert();

            ViewToMapMatrix = transform;
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
        /// Gets scaling factors from meters to view coordinates for a relative map scale.
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

            var transform = new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d);
            transform.Rotate(Rotation);

            return transform;
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

            var transform = new Matrix(transformScale, 0d, 0d, transformScale, 0d, 0d);
            transform.Rotate(Rotation);
            transform.Translate(viewOrigin.X, viewOrigin.Y);

            return transform;
        }

        /// <summary>
        /// Gets the index bounds of a tile matrix.
        /// </summary>
        public Rect GetTileMatrixBounds(double tileMatrixScale, Point tileMatrixTopLeft, Size viewSize)
        {
            // View origin in map coordinates.
            //
            var origin = ViewToMap(new Point());

            var transformScale = tileMatrixScale / Scale;

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
                .TransformBounds(new Rect(0d, 0d, viewSize.Width, viewSize.Height));
        }

        internal static Matrix CreateTransformMatrix(
            double translation1X, double translation1Y,
            double rotation,
            double translation2X, double translation2Y)
        {
            var transform = new Matrix(1d, 0d, 0d, 1d, translation1X, translation1Y);
            transform.Rotate(rotation);
            transform.Translate(translation2X, translation2Y);

            return transform;
        }
    }
}
