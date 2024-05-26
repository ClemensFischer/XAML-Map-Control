// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MapControl
{
    public partial class MapGraticule : Control, IMapElement
    {
        public static readonly StyledProperty<IBrush> ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, IBrush>(TextElement.ForegroundProperty, null,
                (graticule, oldValue, newValue) => graticule.InvalidateVisual());

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly StyledProperty<double> FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, double>(TextElement.FontSizeProperty);

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            DependencyPropertyHelper.AddOwner<MapGraticule, double>(Shape.StrokeThicknessProperty, 0.5);

        public IBrush Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public FontFamily FontFamily
        {
            get => GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
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
                IsFilled = false
            };

            figure.Segments.Add(new PolyLineSegment(points.Skip(1)));
            return figure;
        }
    }
}
