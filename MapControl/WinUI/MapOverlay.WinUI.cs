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
    public class MapOverlay : MapPanel
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

        public static readonly DependencyProperty StrokeLineCapProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineCap>(nameof(StrokeLineCap), PenLineCap.Flat);

        public static readonly DependencyProperty StrokeLineJoinProperty =
            DependencyPropertyHelper.Register<MapOverlay, PenLineJoin>(nameof(StrokeLineJoin), PenLineJoin.Miter);

        public static readonly DependencyProperty StrokeMiterLimitProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeMiterLimit), 1d);

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        public double StrokeDashOffset
        {
            get => (double)GetValue(StrokeDashOffsetProperty);
            set => SetValue(StrokeDashOffsetProperty, value);
        }

        public PenLineCap StrokeLineCap
        {
            get => (PenLineCap)GetValue(StrokeLineCapProperty);
            set => SetValue(StrokeLineCapProperty, value);
        }

        public PenLineJoin StrokeLineJoin
        {
            get => (PenLineJoin)GetValue(StrokeLineJoinProperty);
            set => SetValue(StrokeLineJoinProperty, value);
        }

        public double StrokeMiterLimit
        {
            get => (double)GetValue(StrokeMiterLimitProperty);
            set => SetValue(StrokeMiterLimitProperty, value);
        }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
                if (Foreground == null)
                {
                    SetBinding(ForegroundProperty, map.CreateBinding(nameof(Foreground)));
                }

                if (Stroke == null)
                {
                    SetBinding(StrokeProperty, this.CreateBinding(nameof(Foreground)));
                }
            }

            base.SetParentMap(map);
        }
    }
}
