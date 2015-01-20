// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Location), typeof(MapBase), new PropertyMetadata(new Location(),
                (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Location), typeof(MapBase), new PropertyMetadata(new Location(),
                (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(MapBase), new PropertyMetadata(0d,
                (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(MapBase), new PropertyMetadata(0d,
                (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        partial void Initialize()
        {
            // set Background by Style to enable resetting by ClearValue in RemoveTileLayers
            var style = new Style(typeof(MapBase));
            style.Setters.Add(new Setter(Panel.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            Style = style;

            Clip = new RectangleGeometry();

            SizeChanged += OnRenderSizeChanged;
        }

        private void OnRenderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((RectangleGeometry)Clip).Rect = new Rect(new Point(), e.NewSize);
            ResetTransformOrigin();
            UpdateTransform();
        }

        private void SetViewportTransform(Point mapOrigin)
        {
            viewportTransform.Matrix = Matrix.Identity
                .Translate(-mapOrigin.X, -mapOrigin.Y)
                .Scale(ViewportScale, -ViewportScale)
                .Rotate(Heading)
                .Translate(viewportOrigin.X, viewportOrigin.Y);
        }

        private void SetTileLayerTransform()
        {
            var scale = Math.Pow(2d, ZoomLevel - TileZoomLevel);

            tileLayerTransform.Matrix = Matrix.Identity
                .Translate(TileGrid.X * TileSource.TileSize, TileGrid.Y * TileSource.TileSize)
                .Scale(scale, scale)
                .Translate(tileLayerOffset.X, tileLayerOffset.Y)
                .RotateAt(Heading, viewportOrigin.X, viewportOrigin.Y); ;
        }

        private void SetTransformMatrixes()
        {
            scaleTransform.Matrix = Matrix.Identity.Scale(CenterScale, CenterScale);
            rotateTransform.Matrix = Matrix.Identity.Rotate(Heading);
            scaleRotateTransform.Matrix = scaleTransform.Matrix.Multiply(rotateTransform.Matrix);
        }

        private Matrix GetTileIndexMatrix(double scale)
        {
            return viewportTransform.Matrix
                .Invert() // view to map coordinates
                .Translate(180d, -180d)
                .Scale(scale, -scale); // map coordinates to tile indices
        }
    }
}
