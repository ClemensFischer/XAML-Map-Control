// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Foundation;
using System.Collections.Generic;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    public partial class MapGraticule
    {
        private readonly Path path = new Path { Data = new PathGeometry() };

        public MapGraticule()
        {
            StrokeThickness = 0.5;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            var labels = DrawGraticule(((PathGeometry)path.Data).Figures);

            if (Children.Count == 0)
            {
                path.SetBinding(Shape.StrokeProperty, this.GetOrCreateBinding(StrokeProperty, nameof(Stroke)));
                path.SetBinding(Shape.StrokeThicknessProperty, this.GetOrCreateBinding(StrokeThicknessProperty, nameof(StrokeThickness)));
                path.SetBinding(Shape.StrokeDashArrayProperty, this.GetOrCreateBinding(StrokeDashArrayProperty, nameof(StrokeDashArray)));
                path.SetBinding(Shape.StrokeDashOffsetProperty, this.GetOrCreateBinding(StrokeDashOffsetProperty, nameof(StrokeDashOffset)));
                path.SetBinding(Shape.StrokeDashCapProperty, this.GetOrCreateBinding(StrokeDashCapProperty, nameof(StrokeDashCap)));

                Children.Add(path);
            }

            var childrenCount = 1;

            foreach (var label in labels)
            {
                TextBlock textBlock;

                if (childrenCount < Children.Count)
                {
                    textBlock = (TextBlock)Children[childrenCount];
                }
                else
                {
                    textBlock = new TextBlock { RenderTransform = new MatrixTransform() };
                    textBlock.SetBinding(TextBlock.FontSizeProperty, this.GetOrCreateBinding(FontSizeProperty, nameof(FontSize)));
                    textBlock.SetBinding(TextBlock.FontStyleProperty, this.GetOrCreateBinding(FontStyleProperty, nameof(FontStyle)));
                    textBlock.SetBinding(TextBlock.FontStretchProperty, this.GetOrCreateBinding(FontStretchProperty, nameof(FontStretch)));
                    textBlock.SetBinding(TextBlock.FontWeightProperty, this.GetOrCreateBinding(FontWeightProperty, nameof(FontWeight)));
                    textBlock.SetBinding(TextBlock.ForegroundProperty, this.GetOrCreateBinding(ForegroundProperty, nameof(Foreground)));

                    if (FontFamily != null)
                    {
                        textBlock.SetBinding(TextBlock.FontFamilyProperty, this.GetOrCreateBinding(FontFamilyProperty, nameof(FontFamily)));
                    }

                    Children.Add(textBlock);
                }

                textBlock.Text = label.LatitudeText + "\n" + label.LongitudeText;
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                var matrix = new Matrix(1, 0, 0, 1, 0, 0);

                matrix.Translate(StrokeThickness / 2d + 2d, -textBlock.DesiredSize.Height / 2d);
                matrix.Rotate(label.Rotation);
                matrix.Translate(label.X, label.Y);

                ((MatrixTransform)textBlock.RenderTransform).Matrix = matrix;

                childrenCount++;
            }

            while (Children.Count > childrenCount)
            {
                Children.RemoveAt(Children.Count - 1);
            }

            base.OnViewportChanged(e);
        }

        private static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsFilled = false
            };

            var polyline = new PolyLineSegment();

            foreach (var p in points.Skip(1))
            {
                polyline.Points.Add(p);
            }

            figure.Segments.Add(polyline);
            return figure;
        }
    }
}
