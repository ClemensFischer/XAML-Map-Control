using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SampleApplication
{
    public class OutlinedText : FrameworkElement
    {
        private FormattedText text;
        private Geometry outline;

        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontSizeProperty = TextBlock.FontSizeProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontFamilyProperty = TextBlock.FontFamilyProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontStyleProperty = TextBlock.FontStyleProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontWeightProperty = TextBlock.FontWeightProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontStretchProperty = TextBlock.FontStretchProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty ForegroundProperty = TextBlock.ForegroundProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty BackgroundProperty = TextBlock.BackgroundProperty.AddOwner(
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(Brushes.White, (o, e) => ((OutlinedText)o).text = null) { AffectsMeasure = true });

        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            nameof(OutlineThickness), typeof(double), typeof(OutlinedText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, e) => ((OutlinedText)o).text = null));

        public OutlinedText()
        {
            IsHitTestVisible = false;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

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

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return ValidateText() ? outline.Bounds.Size : new Size();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ValidateText())
            {
                var location = outline.Bounds.Location;
                drawingContext.PushTransform(new TranslateTransform(-location.X, -location.Y));
                drawingContext.DrawGeometry(Background, null, outline);
                drawingContext.DrawText(text, new Point());
            }
        }

        private bool ValidateText()
        {
            if (text == null)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return false;
                }

                var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

                if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                {
                    return false;
                }

                text = new FormattedText(Text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                outline = text.BuildGeometry(new Point()).GetWidenedPathGeometry(
                    new Pen
                    {
                        Thickness = OutlineThickness * 2d,
                        LineJoin = PenLineJoin.Round,
                        StartLineCap = PenLineCap.Round,
                        EndLineCap = PenLineCap.Round
                    });
            }

            return true;
        }
    }
}
