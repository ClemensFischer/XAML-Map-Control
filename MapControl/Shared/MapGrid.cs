using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Media;
using Brush = Avalonia.Media.IBrush;
using PathFigureCollection = Avalonia.Media.PathFigures;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class of map grid or graticule overlays.
    /// </summary>
    public abstract partial class MapGrid
    {
        protected class Label(string text, double x, double y, double rotation)
        {
            public string Text => text;
            public double X => x;
            public double Y => y;
            public double Rotation => rotation;
        }

        public static readonly DependencyProperty MinLineDistanceProperty =
            DependencyPropertyHelper.Register<MapGrid, double>(nameof(MinLineDistance), 150d);

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.Register<MapGrid, double>(nameof(StrokeThickness), 0.5);

        /// <summary>
        /// Minimum graticule line distance in pixels. The default value is 150.
        /// </summary>
        public double MinLineDistance
        {
            get => (double)GetValue(MinLineDistanceProperty);
            set => SetValue(MinLineDistanceProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

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

        protected abstract void DrawGrid(PathFigureCollection figures, List<Label> labels);

        protected static PathFigure CreateLineFigure(Point p1, Point p2)
        {
            var figure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(new LineSegment { Point = p2 });
            return figure;
        }

        protected static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(CreatePolyLineSegment(points.Skip(1)));
            return figure;
        }
    }
}
