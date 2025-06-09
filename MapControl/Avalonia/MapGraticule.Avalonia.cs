using Avalonia.Controls.Primitives;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MapControl
{
    public partial class MapGraticule : TemplatedControl, IMapElement
    {
        static MapGraticule()
        {
            ForegroundProperty.Changed.AddClassHandler<MapGraticule, IBrush>((graticule, e) => graticule.InvalidateVisual());
        }

        private MapBase parentMap;

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get => parentMap;
            set
            {
                if (parentMap != null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                parentMap = value;

                if (parentMap != null)
                {
                    parentMap.ViewportChanged += OnViewportChanged;
                }
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            if (parentMap != null)
            {
                var pathGeometry = new PathGeometry();

                var labels = DrawGraticule(pathGeometry.Figures);

                var pen = new Pen
                {
                    Brush = Foreground,
                    Thickness = StrokeThickness,
                };

                drawingContext.DrawGeometry(null, pen, pathGeometry);

                if (labels.Count > 0)
                {
                    var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

                    foreach (var label in labels)
                    {
                        var latText = new FormattedText(label.LatitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);

                        var lonText = new FormattedText(label.LongitudeText,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);

                        var x = StrokeThickness / 2d + 2d;
                        var y1 = -StrokeThickness / 2d - latText.Height;
                        var y2 = StrokeThickness / 2d;

                        using var pushState = drawingContext.PushTransform(
                            Matrix.CreateRotation(Matrix.ToRadians(label.Rotation)) *
                            Matrix.CreateTranslation(label.X, label.Y));

                        drawingContext.DrawText(latText, new Point(x, y1));
                        drawingContext.DrawText(lonText, new Point(x, y2));
                    }
                }
            }
        }

        private static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(new PolyLineSegment(points.Skip(1)));
            return figure;
        }
    }
}
