// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
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
    /// <summary>
    /// Base class for map overlays with background, foreground, stroke and font properties.
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

        public Binding FontSizeBinding
        {
            get { return GetBinding(FontSizeProperty, nameof(FontSize)); }
        }

        public Binding FontFamilyBinding
        {
            get { return GetBinding(FontFamilyProperty, nameof(FontFamily)); }
        }

        public Binding FontStyleBinding
        {
            get { return GetBinding(FontStyleProperty, nameof(FontStyle)); }
        }

        public Binding FontStretchBinding
        {
            get { return GetBinding(FontStretchProperty, nameof(FontStretch)); }
        }

        public Binding FontWeightBinding
        {
            get { return GetBinding(FontWeightProperty, nameof(FontWeight)); }
        }

        public Binding ForegroundBinding
        {
            get { return GetBinding(ForegroundProperty, nameof(Foreground)); }
        }

        public Binding StrokeBinding
        {
            get { return GetBinding(StrokeProperty, nameof(Stroke)); }
        }

        public Binding StrokeThicknessBinding
        {
            get { return GetBinding(StrokeThicknessProperty, nameof(StrokeThickness)); }
        }

        public Binding StrokeDashArrayBinding
        {
            get { return GetBinding(StrokeDashArrayProperty, nameof(StrokeDashArray)); }
        }

        public Binding StrokeDashOffsetBinding
        {
            get { return GetBinding(StrokeDashOffsetProperty, nameof(StrokeDashOffset)); }
        }

        public Binding StrokeDashCapBinding
        {
            get { return GetBinding(StrokeDashCapProperty, nameof(StrokeDashCap)); }
        }

        public Binding StrokeStartLineCapBinding
        {
            get { return GetBinding(StrokeStartLineCapProperty, nameof(StrokeStartLineCap)); }
        }

        public Binding StrokeEndLineCapBinding
        {
            get { return GetBinding(StrokeEndLineCapProperty, nameof(StrokeEndLineCap)); }
        }

        public Binding StrokeLineJoinBinding
        {
            get { return GetBinding(StrokeLineJoinProperty, nameof(StrokeLineJoin)); }
        }

        public Binding StrokeMiterLimitBinding
        {
            get { return GetBinding(StrokeMiterLimitProperty, nameof(StrokeMiterLimit)); }
        }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
#if WINDOWS_UWP
                if (Foreground == null)
                {
                    SetBinding(ForegroundProperty,
                        map.GetBindingExpression(MapBase.ForegroundProperty)?.ParentBinding ??
                        new Binding { Source = map, Path = new PropertyPath("Foreground") });
                }
#endif
                if (Stroke == null)
                {
                    SetBinding(StrokeProperty, ForegroundBinding);
                }
            }

            base.SetParentMap(map);
        }

        private Binding GetBinding(DependencyProperty property, string propertyName)
        {
            return GetBindingExpression(property)?.ParentBinding ??
                new Binding { Source = this, Path = new PropertyPath(propertyName) };
        }
    }
}
