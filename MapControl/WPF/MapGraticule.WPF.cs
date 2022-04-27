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
            var projection = ParentMap?.MapProjection;

            if (projection != null)
            {
                var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                var pathGeometry = new PathGeometry();
                var labels = new List<Label>();

                SetLineDistance();

                if (projection.Type <= MapProjectionType.NormalCylindrical)
                {
                    DrawCylindricalGraticule(pathGeometry.Figures, labels);
                }
                else
                {
                    DrawGraticule(pathGeometry.Figures, labels);
                }

                drawingContext.DrawGeometry(null, CreatePen(), pathGeometry);

                foreach (var label in labels)
                {
                    var latText = new FormattedText(label.LatitudeText,
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);

                    var lonText = new FormattedText(label.LongitudeText,
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);

                    var x = label.Position.X + StrokeThickness / 2d + 2d;
                    var y1 = label.Position.Y - StrokeThickness / 2d - latText.Height;
                    var y2 = label.Position.Y + StrokeThickness / 2d;

                    drawingContext.PushTransform(new RotateTransform(label.Rotation, label.Position.X, label.Position.Y));
                    drawingContext.DrawText(latText, new Point(x, y1));
                    drawingContext.DrawText(lonText, new Point(x, y2));
                    drawingContext.Pop();
                }
            }
        }

        private static PathFigure CreatePolylineFigure(ICollection<Point> points)
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
