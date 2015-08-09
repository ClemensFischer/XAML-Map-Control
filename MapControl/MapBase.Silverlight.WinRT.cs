// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        private void SetViewportTransform(Location origin)
        {
            MapOrigin = mapTransform.Transform(origin);
            ViewportScale = Math.Pow(2d, ZoomLevel) * TileSource.TileSize / 360d;

            viewportTransform.Matrix =
                new Matrix(1d, 0d, 0d, 1d, -MapOrigin.X, -MapOrigin.Y)
                .Scale(ViewportScale, -ViewportScale)
                .Rotate(Heading)
                .Translate(ViewportOrigin.X, ViewportOrigin.Y);
        }
    }
}
