// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Data;

namespace MapControl
{
    public partial class MapGraticule
    {
        private Path path;

        public MapGraticule()
        {
            IsHitTestVisible = false;
            StrokeThickness = 0.5;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            var projection = ParentMap.MapProjection;

            if (projection.IsNormalCylindrical)
            {
                if (path == null)
                {
                    path = new Path { Data = new PathGeometry() };
                    path.SetBinding(Shape.StrokeProperty, GetBinding(StrokeProperty, nameof(Stroke)));
                    path.SetBinding(Shape.StrokeThicknessProperty, GetBinding(StrokeThicknessProperty, nameof(StrokeThickness)));
                    path.SetBinding(Shape.StrokeDashArrayProperty, GetBinding(StrokeDashArrayProperty, nameof(StrokeDashArray)));
                    path.SetBinding(Shape.StrokeDashOffsetProperty, GetBinding(StrokeDashOffsetProperty, nameof(StrokeDashOffset)));
                    path.SetBinding(Shape.StrokeDashCapProperty, GetBinding(StrokeDashCapProperty, nameof(StrokeDashCap)));
                    Children.Add(path);
                }

                var bounds = projection.ViewportRectToBoundingBox(new Rect(0d, 0d, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height));
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
                        StartPoint = projection.LocationToViewportPoint(new Location(lat, lineStart.Longitude)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = projection.LocationToViewportPoint(new Location(lat, lineEnd.Longitude))
                    });

                    geometry.Figures.Add(figure);
                }

                for (var lon = labelStart.Longitude; lon <= bounds.East; lon += lineDistance)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = projection.LocationToViewportPoint(new Location(lineStart.Latitude, lon)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = projection.LocationToViewportPoint(new Location(lineEnd.Latitude, lon))
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
                            var renderTransform = new TransformGroup();
                            renderTransform.Children.Add(new TranslateTransform());
                            renderTransform.Children.Add(ParentMap.RotateTransform);
                            renderTransform.Children.Add(new TranslateTransform());

                            label = new TextBlock { RenderTransform = renderTransform };
                            if (FontFamily != null)
                            {
                                label.SetBinding(TextBlock.FontFamilyProperty, GetBinding(FontFamilyProperty, nameof(FontFamily)));
                            }
                            label.SetBinding(TextBlock.FontSizeProperty, GetBinding(FontSizeProperty, nameof(FontSize)));
                            label.SetBinding(TextBlock.FontStyleProperty, GetBinding(FontStyleProperty, nameof(FontStyle)));
                            label.SetBinding(TextBlock.FontStretchProperty, GetBinding(FontStretchProperty, nameof(FontStretch)));
                            label.SetBinding(TextBlock.FontWeightProperty, GetBinding(FontWeightProperty, nameof(FontWeight)));
                            label.SetBinding(TextBlock.ForegroundProperty, GetBinding(ForegroundProperty, nameof(Foreground)));

                            Children.Add(label);
                        }

                        childIndex++;

                        label.Text = GetLabelText(lat, labelFormat, "NS") + "\n" + GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW");
                        label.Tag = new Location(lat, lon);
                        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        var translateTransform = (TranslateTransform)((TransformGroup)label.RenderTransform).Children[0];
                        translateTransform.X = StrokeThickness / 2d + 2d;
                        translateTransform.Y = -label.DesiredSize.Height / 2d;
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
                    var viewportTransform = (TranslateTransform)((TransformGroup)label.RenderTransform).Children[2];
                    var viewportPosition = projection.LocationToViewportPoint(location);
                    viewportTransform.X = viewportPosition.X;
                    viewportTransform.Y = viewportPosition.Y;
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
