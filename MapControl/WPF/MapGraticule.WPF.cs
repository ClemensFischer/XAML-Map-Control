// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule : Control, IMapElement
    {
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.Register<MapGraticule, double>(nameof(StrokeThickness), 0.5);

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
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

        protected override void OnRender(DrawingContext drawingContext)
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
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
            return figure;
        }
    }
}
