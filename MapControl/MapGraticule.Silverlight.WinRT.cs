// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Text;
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
    public partial class MapGraticule : MapPanel
    {
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(MapGraticule),
            new PropertyMetadata(default(FontFamily), (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MapGraticule),
            new PropertyMetadata(10d, (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            "FontStyle", typeof(FontStyle), typeof(MapGraticule),
            new PropertyMetadata(default(FontStyle), (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            "FontStretch", typeof(FontStretch), typeof(MapGraticule),
            new PropertyMetadata(default(FontStretch), (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", typeof(FontWeight), typeof(MapGraticule),
            new PropertyMetadata(FontWeights.Normal, (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapGraticule),
            new PropertyMetadata(null, (o, e) => ((MapGraticule)o).OnViewportChanged()));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(MapGraticule),
            new PropertyMetadata(null, (o, e) => ((MapGraticule)o).path.Stroke = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(MapGraticule),
            new PropertyMetadata(0.5, (o, e) => ((MapGraticule)o).path.StrokeThickness = (double)e.NewValue));

        private readonly Path path;
        private Location graticuleStart;
        private Location graticuleEnd;

        public MapGraticule()
        {
            IsHitTestVisible = false;

            path = new Path
            {
                Stroke = Stroke,
                StrokeThickness = StrokeThickness
            };

            Children.Add(path);
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        protected override void OnViewportChanged()
        {
            if (ParentMap != null)
            {
                if (path.Data == null)
                {
                    path.Data = new PathGeometry();

                    if (Foreground == null)
                    {
                        SetBinding(ForegroundProperty, new Binding()
                        {
                            Source = ParentMap,
                            Path = new PropertyPath("Foreground")
                        });
                    }

                    if (Stroke == null)
                    {
                        SetBinding(StrokeProperty, new Binding()
                        {
                            Source = ParentMap,
                            Path = new PropertyPath("Foreground")
                        });
                    }
                }

                var geometry = (PathGeometry)path.Data;
                var bounds = ParentMap.ViewportTransform.Inverse.TransformBounds(new Rect(0d, 0d, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height));
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
                    ParentMap.MapTransform.Transform(lineStart); // get lineStart.TransformedLatitude
                    ParentMap.MapTransform.Transform(lineEnd); // get lineEnd.TransformedLatitude

                    graticuleStart = lineStart;
                    graticuleEnd = lineEnd;

                    geometry.Figures.Clear();
                    geometry.Transform = ParentMap.ViewportTransform;

                    var latLocations = new List<Location>((int)((end.Latitude - labelStart.Latitude) / spacing) + 1);

                    for (var lat = labelStart.Latitude; lat <= end.Latitude; lat += spacing)
                    {
                        var location = new Location(lat, lineStart.Longitude);
                        latLocations.Add(location);

                        var figure = new PathFigure
                        {
                            StartPoint = ParentMap.MapTransform.Transform(location),
                            IsClosed = false,
                            IsFilled = false
                        };

                        location.Longitude = lineEnd.Longitude;

                        figure.Segments.Add(new LineSegment
                        {
                            Point = ParentMap.MapTransform.Transform(location),
                        });

                        geometry.Figures.Add(figure);
                    }

                    for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += spacing)
                    {
                        var figure = new PathFigure
                        {
                            StartPoint = ParentMap.MapTransform.Transform(new Location(lineStart.TransformedLatitude, lineStart.Latitude, lon)),
                            IsClosed = false,
                            IsFilled = false
                        };

                        figure.Segments.Add(new LineSegment
                        {
                            Point = ParentMap.MapTransform.Transform(new Location(lineEnd.TransformedLatitude, lineEnd.Latitude, lon)),
                        });

                        geometry.Figures.Add(figure);
                    }

                    var childIndex = 1; // 0 for Path
                    var format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";
                    var measureSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

                    foreach (var location in latLocations)
                    {
                        for (var lon = labelStart.Longitude; lon <= end.Longitude; lon += spacing)
                        {
                            location.Longitude = lon;

                            TextBlock label;

                            if (childIndex < Children.Count)
                            {
                                label = (TextBlock)Children[childIndex];
                            }
                            else
                            {
                                label = new TextBlock
                                {
                                    RenderTransform = new TransformGroup()
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

                            label.Text = string.Format("{0}\n{1}",
                                CoordinateString(location.Latitude, format, "NS"),
                                CoordinateString(Location.NormalizeLongitude(location.Longitude), format, "EW"));

                            label.Measure(measureSize);

                            var transformGroup = (TransformGroup)label.RenderTransform;

                            if (transformGroup.Children.Count == 0)
                            {
                                transformGroup.Children.Add(new TranslateTransform());
                                transformGroup.Children.Add(ParentMap.RotateTransform);
                                transformGroup.Children.Add(new TranslateTransform());
                            }

                            var translateTransform = (TranslateTransform)transformGroup.Children[0];
                            translateTransform.X = StrokeThickness / 2d + 2d;
                            translateTransform.Y = -label.DesiredSize.Height / 2d;

                            var viewportPosition = ParentMap.LocationToViewportPoint(location);
                            translateTransform = (TranslateTransform)transformGroup.Children[2];
                            translateTransform.X = viewportPosition.X;
                            translateTransform.Y = viewportPosition.Y;
                        }
                    }

                    while (Children.Count > childIndex)
                    {
                        Children.RemoveAt(Children.Count - 1);
                    }
                }
            }
            else
            {
                path.Data = null;
            }

            base.OnViewportChanged();
        }
    }
}
