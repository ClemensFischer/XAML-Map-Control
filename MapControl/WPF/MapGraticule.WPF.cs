// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule
    {
        static MapGraticule()
        {
            StrokeThicknessProperty.OverrideMetadata(typeof(MapGraticule), new FrameworkPropertyMetadata(0.5));
        }

        private readonly PathGeometry pathGeometry = new PathGeometry();

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var pathGeometry = new PathGeometry();

                var labels = DrawGraticule(pathGeometry.Figures);

                drawingContext.DrawGeometry(null, CreatePen(), pathGeometry);

                if (labels.Count > 0)
                {
                    var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
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

        private static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsFilled = false
            };

            figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
            return figure;
        }
    }
}
