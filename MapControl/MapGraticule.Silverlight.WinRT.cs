// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if WINRT
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapGraticule
    {
        private Location graticuleStart;
        private Location graticuleEnd;

        protected override void OnViewportChanged()
        {
            var parentMap = MapPanel.GetParentMap(this);
            var bounds = parentMap.ViewportTransform.Inverse.TransformBounds(new Rect(0d, 0d, parentMap.RenderSize.Width, parentMap.RenderSize.Height));
            var start = parentMap.MapTransform.Transform(new Point(bounds.X, bounds.Y));
            var end = parentMap.MapTransform.Transform(new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            var minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, parentMap.ZoomLevel) * 256d);
            var spacing = LineSpacings[LineSpacings.Length - 1];

            if (spacing >= minSpacing)
            {
                spacing = LineSpacings.FirstOrDefault(s => s >= minSpacing);
            }

            var labelsStart = new Location(
                Math.Ceiling(start.Latitude / spacing) * spacing,
                Math.Ceiling(start.Longitude / spacing) * spacing);

            var labelsEnd = new Location(
                Math.Floor(end.Latitude / spacing) * spacing,
                Math.Floor(end.Longitude / spacing) * spacing);

            var linesStart = new Location(
                Math.Min(Math.Max(labelsStart.Latitude - spacing, -parentMap.MapTransform.MaxLatitude), parentMap.MapTransform.MaxLatitude),
                labelsStart.Longitude - spacing);

            var linesEnd = new Location(
                Math.Min(Math.Max(labelsEnd.Latitude + spacing, -parentMap.MapTransform.MaxLatitude), parentMap.MapTransform.MaxLatitude),
                labelsEnd.Longitude + spacing);

            if (!linesStart.Equals(graticuleStart) || !linesEnd.Equals(graticuleEnd))
            {
                graticuleStart = linesStart;
                graticuleEnd = linesEnd;

                Geometry.Figures.Clear();
                Geometry.Transform = parentMap.ViewportTransform;

                for (var lat = labelsStart.Latitude; lat <= end.Latitude; lat += spacing)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = parentMap.MapTransform.Transform(new Location(lat, linesStart.Longitude)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = parentMap.MapTransform.Transform(new Location(lat, linesEnd.Longitude)),
                    });

                    Geometry.Figures.Add(figure);
                }

                for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = parentMap.MapTransform.Transform(new Location(linesStart.Latitude, lon)),
                        IsClosed = false,
                        IsFilled = false
                    };

                    figure.Segments.Add(new LineSegment
                    {
                        Point = parentMap.MapTransform.Transform(new Location(linesEnd.Latitude, lon)),
                    });

                    Geometry.Figures.Add(figure);
                }

                var childIndex = 1; // 0 for Path

                if (Foreground != null)
                {
                    var format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";
                    var measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

                    for (var lat = labelsStart.Latitude; lat <= end.Latitude; lat += spacing)
                    {
                        for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                        {
                            var location = new Location(lat, lon);
                            TextBlock label;

                            if (childIndex < Children.Count)
                            {
                                label = (TextBlock)Children[childIndex];
                            }
                            else
                            {
                                label = new TextBlock { RenderTransform = new TransformGroup() };
                                Children.Add(label);
                            }

                            childIndex++;

                            if (FontFamily != null)
                            {
                                label.FontFamily = FontFamily;
                            }

                            label.FontSize = FontSize;
                            label.FontStyle = FontStyle;
                            label.FontWeight = FontWeight;
                            label.FontStretch = FontStretch;
                            label.Foreground = Foreground;

                            label.Text = string.Format("{0}\n{1}",
                                CoordinateString(lat, format, "NS"),
                                CoordinateString(Location.NormalizeLongitude(lon), format, "EW"));

                            label.Measure(measureSize);

                            var translateTransform = new TranslateTransform
                            {
                                X = StrokeThickness / 2d + 2d,
                                Y = -label.DesiredSize.Height / 2d
                            };

                            var transform = (TransformGroup)label.RenderTransform;

                            if (transform.Children.Count == 0)
                            {
                                transform.Children.Add(translateTransform);
                                transform.Children.Add(parentMap.RotateTransform);
                            }
                            else
                            {
                                transform.Children[0] = translateTransform;
                                transform.Children[1] = parentMap.RotateTransform;
                            }

                            MapPanel.SetLocation(label, location);
                        }
                    }
                }

                while (Children.Count > childIndex)
                {
                    Children.RemoveAt(Children.Count - 1);
                }
            }

            base.OnViewportChanged();
        }
    }
}
