// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Text;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.Register<MapOverlay, FontFamily>(nameof(FontFamily));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(FontSize), 12d);

        public static readonly DependencyProperty FontStyleProperty =
            DependencyPropertyHelper.Register<MapOverlay, FontStyle>(nameof(FontStyle), FontStyle.Normal);

        public static readonly DependencyProperty FontStretchProperty =
            DependencyPropertyHelper.Register<MapOverlay, FontStretch>(nameof(FontStretch), FontStretch.Normal);

        public static readonly DependencyProperty FontWeightProperty =
            DependencyPropertyHelper.Register<MapOverlay, FontWeight>(nameof(FontWeight), FontWeights.Normal);

        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.Register<MapOverlay, Brush>(nameof(Foreground));

        public static readonly DependencyProperty StrokeProperty =
            DependencyPropertyHelper.Register<MapOverlay, Brush>(nameof(Stroke));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeThickness), 1d);

        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyPropertyHelper.Register<MapOverlay, DoubleCollection>(nameof(StrokeDashArray));

        public static readonly DependencyProperty StrokeDashOffsetProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeDashOffset));

        public static readonly DependencyProperty StrokeDashCapProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineCap>(nameof(StrokeDashCap), PenLineCap.Flat);

        public static readonly DependencyProperty StrokeStartLineCapProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineCap>(nameof(StrokeStartLineCap), PenLineCap.Flat);

        public static readonly DependencyProperty StrokeEndLineCapProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineCap>(nameof(StrokeEndLineCap), PenLineCap.Flat);

        public static readonly DependencyProperty StrokeLineJoinProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineJoin>(nameof(StrokeLineJoin), PenLineJoin.Miter);

        public static readonly DependencyProperty StrokeMiterLimitProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeMiterLimit), 1d);

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
                // If this.Forground is not explicitly set, bind it to map.Foreground.
                //
                this.SetBindingOnUnsetProperty(ForegroundProperty, map, MapBase.ForegroundProperty, nameof(Foreground));

                // If this.Stroke is not explicitly set, bind it to this.Foreground.
                //
                this.SetBindingOnUnsetProperty(StrokeProperty, this, ForegroundProperty, nameof(Foreground));
            }

            base.SetParentMap(map);
        }
    }
}
