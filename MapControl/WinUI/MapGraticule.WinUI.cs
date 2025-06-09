using Windows.Foundation;
using System.Collections.Generic;
using System.Linq;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    public partial class MapGraticule : MapPanel
    {
        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.Register<MapGraticule, Brush>(nameof(Foreground));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.Register<MapGraticule, FontFamily>(nameof(FontFamily));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.Register<MapGraticule, double>(nameof(FontSize), 12d);

        private readonly Path path = new Path { Data = new PathGeometry() };

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null && Foreground == null)
            {
                SetBinding(ForegroundProperty,
                    new Binding { Source = map, Path = new PropertyPath(nameof(Foreground)) });
            }

            base.SetParentMap(map);
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            var labels = DrawGraticule(((PathGeometry)path.Data).Figures);

            if (Children.Count == 0)
            {
                path.SetBinding(Shape.StrokeProperty,
                    new Binding { Source = this, Path = new PropertyPath(nameof(Foreground)) });

                path.SetBinding(Shape.StrokeThicknessProperty,
                    new Binding { Source = this, Path = new PropertyPath(nameof(StrokeThickness)) });

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

                    textBlock.SetBinding(TextBlock.FontSizeProperty,
                        new Binding { Source = this, Path = new PropertyPath(nameof(FontSize)) });

                    textBlock.SetBinding(TextBlock.ForegroundProperty,
                        new Binding { Source = this, Path = new PropertyPath(nameof(Foreground)) });

                    if (FontFamily != null)
                    {
                        textBlock.SetBinding(TextBlock.FontFamilyProperty,
                            new Binding { Source = this, Path = new PropertyPath(nameof(FontFamily)) });
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
                IsClosed = false,
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
