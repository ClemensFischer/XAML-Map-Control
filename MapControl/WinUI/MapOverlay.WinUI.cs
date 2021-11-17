// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
#else
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            nameof(FontFamily), typeof(FontFamily), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            nameof(FontSize), typeof(double), typeof(MapOverlay), new PropertyMetadata(12d));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            nameof(FontStyle), typeof(FontStyle), typeof(MapOverlay), new PropertyMetadata(FontStyle.Normal));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            nameof(FontStretch), typeof(FontStretch), typeof(MapOverlay), new PropertyMetadata(FontStretch.Normal));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            nameof(FontWeight), typeof(FontWeight), typeof(MapOverlay), new PropertyMetadata(FontWeights.Normal));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(Brush), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke), typeof(Brush), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(MapOverlay), new PropertyMetadata(1d));

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            nameof(StrokeDashArray), typeof(DoubleCollection), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register(
            nameof(StrokeDashOffset), typeof(double), typeof(MapOverlay), new PropertyMetadata(0d));

        public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
            nameof(StrokeDashCap), typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
            nameof(StrokeStartLineCap), typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
            nameof(StrokeEndLineCap), typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
            nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(MapOverlay), new PropertyMetadata(PenLineJoin.Miter));

        public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
            nameof(StrokeMiterLimit), typeof(double), typeof(MapOverlay), new PropertyMetadata(1d));

        public MapOverlay()
        {
            IsHitTestVisible = false;
        }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
                this.ValidateProperty(ForegroundProperty, map, nameof(MapBase.Foreground));
                this.ValidateProperty(StrokeProperty, this, nameof(Foreground));
            }

            base.SetParentMap(map);
        }
    }
}
