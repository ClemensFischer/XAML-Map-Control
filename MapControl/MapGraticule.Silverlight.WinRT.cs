// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
#endif

namespace MapControl
{
    public partial class MapGraticule
    {
        private Path path;
        private Location graticuleStart;
        private Location graticuleEnd;

        public MapGraticule()
        {
            IsHitTestVisible = false;
            StrokeThickness = 0.5;
        }

        protected override void OnViewportChanged()
        {
            if (path == null)
            {
                path = new Path
                {
                    Data = new PathGeometry()
                };

                path.SetBinding(Shape.StrokeProperty,
                    GetBindingExpression(StrokeProperty)?.ParentBinding ??
                    new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("Stroke")
                    });

                path.SetBinding(Shape.StrokeThicknessProperty,
                    GetBindingExpression(StrokeThicknessProperty)?.ParentBinding ??
                    new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("StrokeThickness")
                    });

                Children.Add(path);
            }

            var bounds = ParentMap.ViewportTransform.Inverse.TransformBounds(new Rect(new Point(), ParentMap.RenderSize));
            var start = ParentMap.MapTransform.Transform(new Point(bounds.X, bounds.Y));
            var end = ParentMap.MapTransform.Transform(new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            var lineDistance = GetLineDistance();

            var labelStart = new Location(
                Math.Ceiling(start.Latitude / lineDistance) * lineDistance,
                Math.Ceiling(start.Longitude / lineDistance) * lineDistance);

            var labelEnd = new Location(
                Math.Floor(end.Latitude / lineDistance) * lineDistance,
                Math.Floor(end.Longitude / lineDistance) * lineDistance);

            var lineStart = new Location(
                Math.Min(Math.Max(labelStart.Latitude - lineDistance, -ParentMap.MapTransform.MaxLatitude), ParentMap.MapTransform.MaxLatitude),
                labelStart.Longitude - lineDistance);

            var lineEnd = new Location(
                Math.Min(Math.Max(labelEnd.Latitude + lineDistance, -ParentMap.MapTransform.MaxLatitude), ParentMap.MapTransform.MaxLatitude),
                labelEnd.Longitude + lineDistance);

            if (!lineStart.Equals(graticuleStart) || !lineEnd.Equals(graticuleEnd))
            {
                graticuleStart = lineStart;
                graticuleEnd = lineEnd;

                var geometry = (PathGeometry)path.Data;
                geometry.Figures.Clear();
                geometry.Transform = ParentMap.ViewportTransform;

                for (var lat = labelStart.Latitude; lat <= end.Latitude; lat += lineDistance)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = ParentMap.MapTransform.Transform(new Location(lat, lineStart.Longitude)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = ParentMap.MapTransform.Transform(new Location(lat, lineEnd.Longitude)),
                    });

                    geometry.Figures.Add(figure);
                }

                for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += lineDistance)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = ParentMap.MapTransform.Transform(new Location(lineStart.Latitude, lon)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = ParentMap.MapTransform.Transform(new Location(lineEnd.Latitude, lon)),
                    });

                    geometry.Figures.Add(figure);
                }

                var labelFormat = GetLabelFormat(lineDistance);
                var childIndex = 1; // 0 for Path

                for (var lat = labelStart.Latitude; lat <= end.Latitude; lat += lineDistance)
                {
                    for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += lineDistance)
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

                            label = new TextBlock
                            {
                                RenderTransform = renderTransform
                            };

                            label.SetBinding(TextBlock.ForegroundProperty,
                                GetBindingExpression(ForegroundProperty)?.ParentBinding ??
                                new Binding
                                {
                                    Source = this,
                                    Path = new PropertyPath("Foreground")
                                });

                            Children.Add(label);
                        }

                        childIndex++;

                        if (FontFamily != null)
                        {
                            label.FontFamily = FontFamily;
                        }

                        label.FontSize = FontSize;
                        label.FontStyle = FontStyle;
                        label.FontStretch = FontStretch;
                        label.FontWeight = FontWeight;
                        label.Text = GetLabelText(lat, labelFormat, "NS") + "\n" + GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW");
                        label.Tag = new Location(lat, lon);
                        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        var translateTransform = (TranslateTransform)((TransformGroup)label.RenderTransform).Children[0];
                        translateTransform.X = StrokeThickness / 2d + 2d;
                        translateTransform.Y = -label.DesiredSize.Height / 2d;
                    }
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
                var viewportPosition = ParentMap.LocationToViewportPoint(location);
                viewportTransform.X = viewportPosition.X;
                viewportTransform.Y = viewportPosition.Y;
            }

            base.OnViewportChanged();
        }
    }
}
