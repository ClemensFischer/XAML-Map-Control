using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGrid : FrameworkElement, IMapElement
    {
        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, Brush>(TextElement.ForegroundProperty);

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, double>(TextElement.FontSizeProperty, 12d);

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
            OnViewportChanged(e);
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
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

                DrawGrid(pathGeometry.Figures, labels);

                drawingContext.DrawGeometry(null, pen, pathGeometry);

                if (labels.Count > 0)
                {
                    var typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                    foreach (var label in labels)
                    {
                        var text = new FormattedText(label.Text,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);
                        var x = label.X + StrokeThickness / 2d + 2d;
                        var y = label.Y - text.Height / 2d;

                        if (label.Rotation != 0d)
                        {
                            drawingContext.PushTransform(new RotateTransform(label.Rotation, label.X, label.Y));
                            drawingContext.DrawText(text, new Point(x, y));
                            drawingContext.Pop();
                        }
                        else
                        {
                            drawingContext.DrawText(text, new Point(x, y));
                        }
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
