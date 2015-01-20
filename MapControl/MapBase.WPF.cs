// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty =
            Control.ForegroundProperty.AddOwner(typeof(MapBase));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Location), typeof(MapBase), new FrameworkPropertyMetadata(
                new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Location), typeof(MapBase), new FrameworkPropertyMetadata(
                new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        static MapBase()
        {
            UIElement.ClipToBoundsProperty.OverrideMetadata(
                typeof(MapBase), new FrameworkPropertyMetadata(true));

            Panel.BackgroundProperty.OverrideMetadata(
                typeof(MapBase), new FrameworkPropertyMetadata(Brushes.Transparent));
        }

        partial void RemoveAnimation(DependencyProperty property)
        {
            BeginAnimation(property, null);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            ResetTransformOrigin();
            UpdateTransform();
        }

        private void SetViewportTransform(Point mapOrigin)
        {
            var transform = Matrix.Identity;
            transform.Translate(-mapOrigin.X, -mapOrigin.Y);
            transform.Scale(ViewportScale, -ViewportScale);
            transform.Rotate(Heading);
            transform.Translate(viewportOrigin.X, viewportOrigin.Y);

            viewportTransform.Matrix = transform;
        }

        private void SetTileLayerTransform()
        {
            var scale = Math.Pow(2d, ZoomLevel - TileZoomLevel);
            var transform = Matrix.Identity;
            transform.Translate(TileGrid.X * TileSource.TileSize, TileGrid.Y * TileSource.TileSize);
            transform.Scale(scale, scale);
            transform.Translate(tileLayerOffset.X, tileLayerOffset.Y);
            transform.RotateAt(Heading, viewportOrigin.X, viewportOrigin.Y);

            tileLayerTransform.Matrix = transform;
        }

        private void SetTransformMatrixes()
        {
            var rotateMatrix = Matrix.Identity;
            rotateMatrix.Rotate(Heading);
            rotateTransform.Matrix = rotateMatrix;

            var scaleMatrix = Matrix.Identity;
            scaleMatrix.Scale(CenterScale, CenterScale);
            scaleTransform.Matrix = scaleMatrix;

            scaleRotateTransform.Matrix = scaleMatrix * rotateMatrix;
        }

        private Matrix GetTileIndexMatrix(double scale)
        {
            var transform = viewportTransform.Matrix;
            transform.Invert(); // view to map coordinates
            transform.Translate(180d, -180d);
            transform.Scale(scale, -scale); // map coordinates to tile indices

            return transform;
        }
    }
}
