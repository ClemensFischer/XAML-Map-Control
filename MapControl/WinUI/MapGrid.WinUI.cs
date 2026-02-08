using Windows.Foundation;
using System.Collections.Generic;
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
    public partial class MapGrid : MapPanel
    {
        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.Register<MapGrid, Brush>(nameof(Foreground));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.Register<MapGrid, FontFamily>(nameof(FontFamily));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.Register<MapGrid, double>(nameof(FontSize), 12d);

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
            Path path;

            if (Children.Count == 0)
            {
                path = new Path { Data = new PathGeometry() };

                path.SetBinding(Shape.StrokeProperty,
                    new Binding { Source = this, Path = new PropertyPath(nameof(Foreground)) });

                path.SetBinding(Shape.StrokeThicknessProperty,
                    new Binding { Source = this, Path = new PropertyPath(nameof(StrokeThickness)) });

                Children.Add(path);
            }
            else
            {
                path = (Path)Children[0];
            }

            var childrenCount = 1;
            var labels = new List<Label>();
            var figures = ((PathGeometry)path.Data).Figures;
            figures.Clear();

            DrawGrid(figures, labels);

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

                textBlock.Text = label.Text;
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                var x = label.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => 2d,
                    HorizontalAlignment.Right => -textBlock.DesiredSize.Width - 2d,
                    _ => -textBlock.DesiredSize.Width / 2d
                };
                var y = label.VerticalAlignment switch
                {
                    VerticalAlignment.Top => 0d,
                    VerticalAlignment.Bottom => -textBlock.DesiredSize.Height,
                    _ => -textBlock.DesiredSize.Height / 2d,
                };

                var matrix = new Matrix(1, 0, 0, 1, 0, 0);
                matrix.Translate(x, y);
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

        private static PolyLineSegment CreatePolyLineSegment(IEnumerable<Point> points)
        {
            var polyline = new PolyLineSegment();

            foreach (var p in points)
            {
                polyline.Points.Add(p);
            }

            return polyline;
        }
    }
}
