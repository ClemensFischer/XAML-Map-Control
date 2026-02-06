using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule : FrameworkElement, IMapElement
    {
        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, Brush>(TextElement.ForegroundProperty);

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, double>(TextElement.FontSizeProperty, 12d);

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get;
            set
            {
                if (field != null)
                {
                    field.ViewportChanged -= OnViewportChanged;
                }

                field = value;

                if (field != null)
                {
                    field.ViewportChanged += OnViewportChanged;
                }
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var pathGeometry = new PathGeometry();
                var labels = new List<Label>();
                var pen = new Pen
                {
                    Brush = Foreground,
                    Thickness = StrokeThickness,
                };

                DrawGraticule(pathGeometry.Figures, labels);

                drawingContext.DrawGeometry(null, pen, pathGeometry);

                if (labels.Count > 0)
                {
                    var typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                    foreach (var label in labels)
                    {
                        var latText = new FormattedText(label.LatitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);

                        var lonText = new FormattedText(label.LongitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);

                        var x = label.X + StrokeThickness / 2d + 2d;
                        var y1 = label.Y - StrokeThickness / 2d - latText.Height;
                        var y2 = label.Y + StrokeThickness / 2d;

                        drawingContext.PushTransform(new RotateTransform(label.Rotation, label.X, label.Y));
                        drawingContext.DrawText(latText, new Point(x, y1));
                        drawingContext.DrawText(lonText, new Point(x, y2));
                        drawingContext.Pop();
                    }
                }
            }
        }

        private static PolyLineSegment CreatePolyLineSegment(IEnumerable<Point> points)
        {
            return new PolyLineSegment(points, true);
        }
    }
}
