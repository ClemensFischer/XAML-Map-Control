// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            nameof(Center), typeof(Location), typeof(MapBase),
            new PropertyMetadata(new Location(), (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            nameof(TargetCenter), typeof(Location), typeof(MapBase),
            new PropertyMetadata(new Location(), (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            nameof(ZoomLevel), typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            nameof(TargetZoomLevel), typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            nameof(Heading), typeof(double), typeof(MapBase),
            new PropertyMetadata(0d, (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            nameof(TargetHeading), typeof(double), typeof(MapBase),
            new PropertyMetadata(0d, (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty ViewScaleProperty = DependencyProperty.Register(
            nameof(ViewScale), typeof(double), typeof(MapBase), new PropertyMetadata(0d));

        internal static readonly DependencyProperty CenterPointProperty = DependencyProperty.Register(
            "CenterPoint", typeof(Windows.Foundation.Point), typeof(MapBase),
            new PropertyMetadata(new Windows.Foundation.Point(), (o, e) =>
            {
                var center = (Windows.Foundation.Point)e.NewValue;
                ((MapBase)o).CenterPointPropertyChanged(new Location(center.Y, center.X));
            }));

        public MapBase()
        {
            // set Background by Style to enable resetting by ClearValue in MapLayerPropertyChanged
            var style = new Style(typeof(MapBase));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.White)));
            Style = style;

            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry
            {
                Rect = new Windows.Foundation.Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height)
            };

            ResetTransformCenter();
            UpdateTransform();
        }

        private void SetViewScale(double scale)
        {
            SetValue(ViewScaleProperty, scale);
        }
    }
}
