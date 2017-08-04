// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(Brush), typeof(MapBase),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

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

        partial void Initialize()
        {
            // set Background by Style to enable resetting by ClearValue in MapLayerPropertyChanged
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

                    ResetTransformCenter();
                    UpdateTransform();
                }
            };
        }
    }
}
