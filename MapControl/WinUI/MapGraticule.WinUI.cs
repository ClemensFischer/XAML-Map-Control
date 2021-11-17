// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.Foundation;
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
        private Path path;

        public MapGraticule()
        {
            StrokeThickness = 0.5;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            var map = ParentMap;
            var projection = map.MapProjection;

            if (projection.IsNormalCylindrical)
            {
                if (path == null)
                {
                    path = new Path { Data = new PathGeometry() };
                    path.SetBinding(Shape.StrokeProperty, this.GetBinding(nameof(Stroke)));
                    path.SetBinding(Shape.StrokeThicknessProperty, this.GetBinding(nameof(StrokeThickness)));
                    path.SetBinding(Shape.StrokeDashArrayProperty, this.GetBinding(nameof(StrokeDashArray)));
                    path.SetBinding(Shape.StrokeDashOffsetProperty, this.GetBinding(nameof(StrokeDashOffset)));
                    path.SetBinding(Shape.StrokeDashCapProperty, this.GetBinding(nameof(StrokeDashCap)));
                    Children.Add(path);
                }

                var bounds = map.ViewRectToBoundingBox(new Rect(0d, 0d, map.RenderSize.Width, map.RenderSize.Height));
                var lineDistance = GetLineDistance();

                var labelStart = new Location(
                    Math.Ceiling(bounds.South / lineDistance) * lineDistance,
                    Math.Ceiling(bounds.West / lineDistance) * lineDistance);

                var labelEnd = new Location(
                    Math.Floor(bounds.North / lineDistance) * lineDistance,
                    Math.Floor(bounds.East / lineDistance) * lineDistance);

                var lineStart = new Location(
                    Math.Min(Math.Max(labelStart.Latitude - lineDistance, -projection.MaxLatitude), projection.MaxLatitude),
                    labelStart.Longitude - lineDistance);

                var lineEnd = new Location(
                    Math.Min(Math.Max(labelEnd.Latitude + lineDistance, -projection.MaxLatitude), projection.MaxLatitude),
                    labelEnd.Longitude + lineDistance);

                var geometry = (PathGeometry)path.Data;
                geometry.Figures.Clear();

                for (var lat = labelStart.Latitude; lat <= bounds.North; lat += lineDistance)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = map.LocationToView(new Location(lat, lineStart.Longitude)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = map.LocationToView(new Location(lat, lineEnd.Longitude))
                    });

                    geometry.Figures.Add(figure);
                }

                for (var lon = labelStart.Longitude; lon <= bounds.East; lon += lineDistance)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = map.LocationToView(new Location(lineStart.Latitude, lon)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = map.LocationToView(new Location(lineEnd.Latitude, lon))
                    });

                    geometry.Figures.Add(figure);
                }

                var labelFormat = GetLabelFormat(lineDistance);
                var childIndex = 1; // 0 for Path

                for (var lat = labelStart.Latitude; lat <= bounds.North; lat += lineDistance)
                {
                    for (var lon = labelStart.Longitude; lon <= bounds.East; lon += lineDistance)
                    {
                        TextBlock label;

                        if (childIndex < Children.Count)
                        {
                            label = (TextBlock)Children[childIndex];
                        }
                        else
                        {
                            label = new TextBlock { RenderTransform = new MatrixTransform() };
                            label.SetBinding(TextBlock.FontSizeProperty, this.GetBinding(nameof(FontSize)));
                            label.SetBinding(TextBlock.FontStyleProperty, this.GetBinding(nameof(FontStyle)));
                            label.SetBinding(TextBlock.FontStretchProperty, this.GetBinding(nameof(FontStretch)));
                            label.SetBinding(TextBlock.FontWeightProperty, this.GetBinding(nameof(FontWeight)));
                            label.SetBinding(TextBlock.ForegroundProperty, this.GetBinding(nameof(Foreground)));

                            if (FontFamily != null)
                            {
                                label.SetBinding(TextBlock.FontFamilyProperty, this.GetBinding(nameof(FontFamily)));
                            }

                            Children.Add(label);
                        }

                        childIndex++;

                        label.Text = GetLabelText(lat, labelFormat, "NS") + "\n" + GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW");
                        label.Tag = new Location(lat, lon);
                        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }

                    while (Children.Count > childIndex)
                    {
                        Children.RemoveAt(Children.Count - 1);
                    }
                }

                // don't use MapPanel.Location because labels may be at more than 180° distance from map center

                for (int i = 1; i < Children.Count; i++)
                {
                    var label = (TextBlock)Children[i];
                    var location = (Location)label.Tag;
                    var viewPosition = map.LocationToView(location);
                    var matrix = new Matrix(1, 0, 0, 1, 0, 0);

                    matrix.Translate(StrokeThickness / 2d + 2d, -label.DesiredSize.Height / 2d);
                    matrix.Rotate(map.ViewTransform.Rotation);
                    matrix.Translate(viewPosition.X, viewPosition.Y);

                    ((MatrixTransform)label.RenderTransform).Matrix = matrix;
                }
            }
            else if (path != null)
            {
                path = null;
                Children.Clear();
            }

            base.OnViewportChanged(e);
        }
    }
}
