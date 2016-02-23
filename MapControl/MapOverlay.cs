// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Text;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map overlays with font, background, foreground and stroke properties.
    /// </summary>
    public partial class MapOverlay : MapPanel
    {
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }

        public double StrokeDashOffset
        {
            get { return (double)GetValue(StrokeDashOffsetProperty); }
            set { SetValue(StrokeDashOffsetProperty, value); }
        }

        public PenLineCap StrokeDashCap
        {
            get { return (PenLineCap)GetValue(StrokeDashCapProperty); }
            set { SetValue(StrokeDashCapProperty, value); }
        }

        public PenLineCap StrokeStartLineCap
        {
            get { return (PenLineCap)GetValue(StrokeStartLineCapProperty); }
            set { SetValue(StrokeStartLineCapProperty, value); }
        }

        public PenLineCap StrokeEndLineCap
        {
            get { return (PenLineCap)GetValue(StrokeEndLineCapProperty); }
            set { SetValue(StrokeEndLineCapProperty, value); }
        }

        public PenLineJoin StrokeLineJoin
        {
            get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
            set { SetValue(StrokeLineJoinProperty, value); }
        }

        public double StrokeMiterLimit
        {
            get { return (double)GetValue(StrokeMiterLimitProperty); }
            set { SetValue(StrokeMiterLimitProperty, value); }
        }
    }
}
