// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    public partial class MapOverlay : MapPanel
    {
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(MapOverlay),
            new PropertyMetadata(default(FontFamily), (o, e) => ((MapOverlay)o).FontSizePropertyChanged()));

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MapOverlay),
            new PropertyMetadata(10d, (o, e) => ((MapOverlay)o).FontSizePropertyChanged()));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            "FontStyle", typeof(FontStyle), typeof(MapOverlay),
            new PropertyMetadata(default(FontStyle), (o, e) => ((MapOverlay)o).FontSizePropertyChanged()));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            "FontStretch", typeof(FontStretch), typeof(MapOverlay),
            new PropertyMetadata(default(FontStretch), (o, e) => ((MapOverlay)o).FontSizePropertyChanged()));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", typeof(FontWeight), typeof(MapOverlay),
            new PropertyMetadata(FontWeights.Normal, (o, e) => ((MapOverlay)o).FontSizePropertyChanged()));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapOverlay),
            new PropertyMetadata(new SolidColorBrush(Colors.Black), (o, e) => ((MapOverlay)o).ForegroundPropertyChanged()));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(MapOverlay),
            new PropertyMetadata(new SolidColorBrush(Colors.Black), (o, e) => ((MapOverlay)o).Path.Stroke = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(MapOverlay),
            new PropertyMetadata(1d, (o, e) => ((MapOverlay)o).StrokeThicknessPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            "StrokeDashArray", typeof(DoubleCollection), typeof(MapOverlay),
            new PropertyMetadata(null, (o, e) => ((MapOverlay)o).Path.StrokeDashArray = (DoubleCollection)e.NewValue));

        public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register(
            "StrokeDashOffset", typeof(double), typeof(MapOverlay),
            new PropertyMetadata(0d, (o, e) => ((MapOverlay)o).Path.StrokeDashOffset = (double)e.NewValue));

        public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
            "StrokeDashCap", typeof(PenLineCap), typeof(MapOverlay),
            new PropertyMetadata(default(PenLineCap), (o, e) => ((MapOverlay)o).Path.StrokeDashCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
            "StrokeStartLineCap", typeof(PenLineCap), typeof(MapOverlay),
            new PropertyMetadata(default(PenLineCap), (o, e) => ((MapOverlay)o).Path.StrokeStartLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
            "StrokeEndLineCap", typeof(PenLineCap), typeof(MapOverlay),
            new PropertyMetadata(default(PenLineCap), (o, e) => ((MapOverlay)o).Path.StrokeEndLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
            "StrokeLineJoin", typeof(PenLineJoin), typeof(MapOverlay),
            new PropertyMetadata(default(PenLineJoin), (o, e) => ((MapOverlay)o).Path.StrokeLineJoin = (PenLineJoin)e.NewValue));

        public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
            "StrokeMiterLimit", typeof(double), typeof(MapOverlay),
            new PropertyMetadata(1d, (o, e) => ((MapOverlay)o).Path.StrokeMiterLimit = (double)e.NewValue));

        protected readonly Path Path = new Path();
        protected readonly PathGeometry Geometry = new PathGeometry();

        public MapOverlay()
        {
            IsHitTestVisible = false;
            Path.Stroke = Stroke;
            Path.StrokeThickness = StrokeThickness;
            Path.StrokeDashArray = StrokeDashArray;
            Path.StrokeDashOffset = StrokeDashOffset;
            Path.StrokeDashCap = StrokeDashCap;
            Path.StrokeStartLineCap = StrokeStartLineCap;
            Path.StrokeEndLineCap = StrokeEndLineCap;
            Path.StrokeLineJoin = StrokeLineJoin;
            Path.StrokeMiterLimit = StrokeMiterLimit;
            Path.Data = Geometry;
            Children.Add(Path);
        }

        private void FontSizePropertyChanged()
        {
            if (GetParentMap(this) != null)
            {
                // FontSize may affect layout
                OnViewportChanged();
            }
        }

        private void ForegroundPropertyChanged()
        {
            if (GetParentMap(this) != null)
            {
                // Foreground may affect rendering
                OnViewportChanged();
            }
        }

        private void StrokeThicknessPropertyChanged(double thickness)
        {
            Path.StrokeThickness = thickness;

            if (GetParentMap(this) != null)
            {
                // StrokeThickness may affect layout
                OnViewportChanged();
            }
        }
    }
}
