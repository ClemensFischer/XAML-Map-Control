// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
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
    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MapOverlay), new PropertyMetadata(10d));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(MapOverlay), new PropertyMetadata(default(FontFamily)));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            "FontStyle", typeof(FontStyle), typeof(MapOverlay), new PropertyMetadata(default(FontStyle)));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            "FontStretch", typeof(FontStretch), typeof(MapOverlay), new PropertyMetadata(default(FontStretch)));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", typeof(FontWeight), typeof(MapOverlay), new PropertyMetadata(default(FontWeight)));

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
            "StrokeDashCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(default(PenLineCap)));

        public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
            "StrokeStartLineCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(default(PenLineCap)));

        public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
            "StrokeEndLineCap", typeof(PenLineCap), typeof(MapOverlay), new PropertyMetadata(default(PenLineCap)));

        public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
            "StrokeLineJoin", typeof(PenLineJoin), typeof(MapOverlay), new PropertyMetadata(default(PenLineJoin)));

        public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
            "StrokeMiterLimit", typeof(double), typeof(MapOverlay), new PropertyMetadata(1d));

        private Binding foregroundBinding;
        private Binding strokeBinding;

        protected override void SetParentMapOverride(MapBase parentMap)
        {
            if (foregroundBinding != null)
            {
                foregroundBinding = null;
                ClearValue(ForegroundProperty);
            }

            if (strokeBinding != null)
            {
                strokeBinding = null;
                ClearValue(StrokeProperty);
            }

            if (parentMap != null)
            {
                if (Foreground == null)
                {
                    foregroundBinding = new Binding
                    {
                        Source = parentMap,
                        Path = new PropertyPath("Foreground")
                    };
                    SetBinding(ForegroundProperty, foregroundBinding);
                }

                if (Stroke == null)
                {
                    strokeBinding = new Binding
                    {
                        Source = parentMap,
                        Path = new PropertyPath("Foreground")
                    };
                    SetBinding(StrokeProperty, strokeBinding);
                }
            }

            base.SetParentMapOverride(parentMap);
        }
    }
}
