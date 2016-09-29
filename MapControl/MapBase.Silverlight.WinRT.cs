// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapBase),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Location), typeof(MapBase),
            new PropertyMetadata(new Location(), (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Location), typeof(MapBase),
            new PropertyMetadata(new Location(), (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(MapBase),
            new PropertyMetadata(0d, (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(MapBase),
            new PropertyMetadata(0d, (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        partial void Initialize()
        {
            // set Background by Style to enable resetting by ClearValue in RemoveTileLayers
            var style = new Style(typeof(MapBase));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            Style = style;

            var clip = new RectangleGeometry();
            Clip = clip;

            SizeChanged += (s, e) =>
            {
                if (clip.Rect.Width != e.NewSize.Width || clip.Rect.Height != e.NewSize.Height)
                {
                    clip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);

                    ResetTransformOrigin();
                    UpdateTransform();
                }
            };
        }

        private void SetViewportTransform(Location origin)
        {
            MapOrigin = mapTransform.Transform(origin);
            ViewportScale = Math.Pow(2d, ZoomLevel) * (double)TileSource.TileSize / 360d;

            var transform = new Matrix(1d, 0d, 0d, 1d, -MapOrigin.X, -MapOrigin.Y)
                .Rotate(-Heading)
                .Scale(ViewportScale, -ViewportScale)
                .Translate(ViewportOrigin.X, ViewportOrigin.Y);

            viewportTransform.Matrix = transform;
        }

        private void SetTileLayer(TileLayer tileLayer)
        {
            TileLayer = tileLayer;
        }
    }
}
