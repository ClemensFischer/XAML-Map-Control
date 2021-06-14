// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty =
            Control.ForegroundProperty.AddOwner(typeof(MapBase));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            nameof(Center), typeof(Location), typeof(MapBase), new FrameworkPropertyMetadata(
                new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            nameof(TargetCenter), typeof(Location), typeof(MapBase), new FrameworkPropertyMetadata(
                new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            nameof(ZoomLevel), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            nameof(TargetZoomLevel), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            nameof(Heading), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            nameof(TargetHeading), typeof(double), typeof(MapBase), new FrameworkPropertyMetadata(
                0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        private static readonly DependencyPropertyKey ViewScalePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ViewScale), typeof(double), typeof(MapBase), new PropertyMetadata(0d));

        public static readonly DependencyProperty ViewScaleProperty = ViewScalePropertyKey.DependencyProperty;

        private static readonly DependencyProperty CenterPointProperty = DependencyProperty.Register(
            "CenterPoint", typeof(Point), typeof(MapBase), new PropertyMetadata(new Point(),
                (o, e) =>
                {
                    var center = (Point)e.NewValue;
                    ((MapBase)o).CenterPointPropertyChanged(new Location(center.Y, center.X));
                }));

        static MapBase()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(MapBase), new FrameworkPropertyMetadata(true));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapBase), new FrameworkPropertyMetadata(typeof(MapBase)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            ResetTransformCenter();
            UpdateTransform();
        }

        private void SetViewScale(double scale)
        {
            SetValue(ViewScalePropertyKey, scale);
        }
    }
}
