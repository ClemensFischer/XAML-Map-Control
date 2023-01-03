// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
#elif UWP
using Windows.UI.Text;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map overlays with background, foreground, stroke and font properties.
    /// </summary>
    public partial class MapOverlay : MapPanel
    {
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

        public PenLineCap StrokeDashCap
        {
            get => (PenLineCap)GetValue(StrokeDashCapProperty);
            set => SetValue(StrokeDashCapProperty, value);
        }

        public PenLineCap StrokeStartLineCap
        {
            get => (PenLineCap)GetValue(StrokeStartLineCapProperty);
            set => SetValue(StrokeStartLineCapProperty, value);
        }

        public PenLineCap StrokeEndLineCap
        {
            get => (PenLineCap)GetValue(StrokeEndLineCapProperty);
            set => SetValue(StrokeEndLineCapProperty, value);
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
    }
}
