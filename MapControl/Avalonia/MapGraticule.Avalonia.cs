using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;

namespace MapControl
{
    public partial class MapGraticule : Control, IMapElement
    {
        static MapGraticule()
        {
            AffectsRender<MapGraticule>(ForegroundProperty);
        }

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, IBrush>(TextElement.ForegroundProperty);

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly StyledProperty<double> FontSizeProperty =
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

        public override void Render(DrawingContext drawingContext)
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
                    var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);

                    foreach (var label in labels)
                    {
                        var latText = new FormattedText(label.LatitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);

                        var lonText = new FormattedText(label.LongitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);

                        var x = StrokeThickness / 2d + 2d;
                        var y1 = -StrokeThickness / 2d - latText.Height;
                        var y2 = StrokeThickness / 2d;

                        var transform = new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
                        transform.Rotate(label.Rotation);
                        transform.Translate(label.X, label.Y);

                        using var pushState = drawingContext.PushTransform(transform);

                        drawingContext.DrawText(latText, new Point(x, y1));
                        drawingContext.DrawText(lonText, new Point(x, y2));
                    }
                }
            }
        }

        private static PolyLineSegment CreatePolyLineSegment(IEnumerable<Point> points)
        {
            return new PolyLineSegment(points);
        }
    }
}
