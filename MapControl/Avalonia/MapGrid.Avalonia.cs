using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MapControl
{
    public partial class MapGrid : Control, IMapElement
    {
        static MapGrid()
        {
            AffectsRender<MapGrid>(ForegroundProperty);
        }

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, IBrush>(TextElement.ForegroundProperty);

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly StyledProperty<double> FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapGrid, double>(TextElement.FontSizeProperty, 12d);

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
            OnViewportChanged(e);
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
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

                DrawGrid(pathGeometry.Figures, labels);

                drawingContext.DrawGeometry(null, pen, pathGeometry);

                if (labels.Count > 0)
                {
                    var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);

                    foreach (var label in labels)
                    {
                        var text = new FormattedText(label.Text,
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);
                        var x = label.X +
                            label.HorizontalAlignment switch
                            {
                                HorizontalAlignment.Left => 2d,
                                HorizontalAlignment.Right => -text.Width - 2d,
                                _ => -text.Width / 2d
                            };
                        var y = label.Y +
                            label.VerticalAlignment switch
                            {
                                VerticalAlignment.Top => 0,
                                VerticalAlignment.Bottom => -text.Height,
                                _ => -text.Height / 2d
                            };

                        if (label.Rotation != 0d)
                        {
                            var transform = Avalonia.Matrix.CreateRotation(
                                label.Rotation * Math.PI / 180d, new Point(label.X, label.Y));

                            using var pushedState = drawingContext.PushTransform(transform);

                            drawingContext.DrawText(text, new Point(x, y));
                        }
                        else
                        {
                            drawingContext.DrawText(text, new Point(x, y));
                        }
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
