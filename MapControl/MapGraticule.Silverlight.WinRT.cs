// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI;
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
        private readonly Path path;
        private Location graticuleStart;
        private Location graticuleEnd;

        public MapGraticule()
        {
            IsHitTestVisible = false;
            StrokeThickness = 0.5;

            path = new Path
            {
                Data = new PathGeometry()
            };

            path.SetBinding(Shape.StrokeProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("Stroke")
            });

            path.SetBinding(Shape.StrokeThicknessProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("StrokeThickness")
            });

            Children.Add(path);
        }

        protected override void OnViewportChanged()
        {
            var bounds = ParentMap.ViewportTransform.Inverse.TransformBounds(new Rect(new Point(), ParentMap.RenderSize));
            var start = ParentMap.MapTransform.Transform(new Point(bounds.X, bounds.Y));
            var end = ParentMap.MapTransform.Transform(new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            var minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, ParentMap.ZoomLevel) * 256d);
            var spacing = LineSpacings[LineSpacings.Length - 1];

            if (spacing >= minSpacing)
            {
                spacing = LineSpacings.FirstOrDefault(s => s >= minSpacing);
            }

            var labelStart = new Location(
                Math.Ceiling(start.Latitude / spacing) * spacing,
                Math.Ceiling(start.Longitude / spacing) * spacing);

            var labelEnd = new Location(
                Math.Floor(end.Latitude / spacing) * spacing,
                Math.Floor(end.Longitude / spacing) * spacing);

            var lineStart = new Location(
                Math.Min(Math.Max(labelStart.Latitude - spacing, -ParentMap.MapTransform.MaxLatitude), ParentMap.MapTransform.MaxLatitude),
                labelStart.Longitude - spacing);

            var lineEnd = new Location(
                Math.Min(Math.Max(labelEnd.Latitude + spacing, -ParentMap.MapTransform.MaxLatitude), ParentMap.MapTransform.MaxLatitude),
                labelEnd.Longitude + spacing);

            if (!lineStart.Equals(graticuleStart) || !lineEnd.Equals(graticuleEnd))
            {
                graticuleStart = lineStart;
                graticuleEnd = lineEnd;

                var geometry = (PathGeometry)path.Data;
                geometry.Figures.Clear();
                geometry.Transform = ParentMap.ViewportTransform;

                for (var lat = labelStart.Latitude; lat <= end.Latitude; lat += spacing)
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

                for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += spacing)
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

                var childIndex = 1; // 0 for Path
                var format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

                for (var lat = labelStart.Latitude; lat <= end.Latitude; lat += spacing)
                {
                    for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += spacing)
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

                            label.SetBinding(TextBlock.ForegroundProperty, new Binding
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
                        label.Text = string.Format("{0}\n{1}", CoordinateString(lat, format, "NS"), CoordinateString(Location.NormalizeLongitude(lon), format, "EW"));
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
