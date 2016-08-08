// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
#endif

namespace MapControl
{
#if NETFX_CORE
    class FontStyles { public const FontStyle Normal = FontStyle.Normal; }
    class FontStretches { public const FontStretch Normal = FontStretch.Normal; }
#endif

    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MapOverlay), new PropertyMetadata(10d));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            "FontStyle", typeof(FontStyle), typeof(MapOverlay), new PropertyMetadata(FontStyles.Normal));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            "FontStretch", typeof(FontStretch), typeof(MapOverlay), new PropertyMetadata(FontStretches.Normal));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", typeof(FontWeight), typeof(MapOverlay), new PropertyMetadata(FontWeights.Normal));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(MapOverlay), new PropertyMetadata(1d));

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            "StrokeDashArray", typeof(DoubleCollection), typeof(MapOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register(
            "StrokeDashOffset", typeof(double), typeof(MapOverlay), new PropertyMetadata(0d));

        public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
            "StrokeDashCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
            "StrokeStartLineCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
            "StrokeEndLineCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(PenLineCap.Flat));

        public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
            "StrokeLineJoin", typeof(PenLineJoin), typeof(MapOverlay), new PropertyMetadata(PenLineJoin.Miter));

        public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
            "StrokeMiterLimit", typeof(double), typeof(MapOverlay), new PropertyMetadata(1d));

        protected override void SetParentMapOverride(MapBase parentMap)
        {
            if (GetBindingExpression(ForegroundProperty) != null)
            {
                ClearValue(ForegroundProperty);
            }

            if (GetBindingExpression(StrokeProperty) != null)
            {
                ClearValue(StrokeProperty);
            }

            if (parentMap != null)
            {
                if (Foreground == null)
                {
                    SetBinding(ForegroundProperty, new Binding
                    {
                        Source = parentMap,
                        Path = new PropertyPath("Foreground")
                    });
                }

                if (Stroke == null)
                {
                    SetBinding(StrokeProperty, new Binding
                    {
                        Source = parentMap,
                        Path = new PropertyPath("Foreground")
                    });
                }
            }

            base.SetParentMapOverride(parentMap);
        }
    }
}
