using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Draws a graticule overlay.
    /// </summary>
    public partial class MapGraticule
    {
        private class Label(string latText, string lonText, double x, double y, double rotation)
        {
            public string LatitudeText => latText;
            public string LongitudeText => lonText;
            public double X => x;
            public double Y => y;
            public double Rotation => rotation;
        }

        public static readonly DependencyProperty MinLineDistanceProperty =
            DependencyPropertyHelper.Register<MapGraticule, double>(nameof(MinLineDistance), 150d);

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.Register<MapGraticule, double>(nameof(StrokeThickness), 0.5);

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

        private List<Label> DrawGraticule(PathFigureCollection figures)
        {
            figures.Clear();

            var labels = new List<Label>();
            var lineDistance = GetLineDistance();
            var labelFormat = lineDistance < 1d / 60d ? "{0} {1}°{2:00}'{3:00}\""
                            : lineDistance < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

            if (ParentMap.MapProjection.IsNormalCylindrical)
            {
                DrawCylindricalGraticule(figures, labels, lineDistance, labelFormat);
            }
            else
            {
                DrawGraticule(figures, labels, lineDistance, labelFormat);
            }

            return labels;
        }

        private void DrawCylindricalGraticule(PathFigureCollection figures, List<Label> labels, double lineDistance, string labelFormat)
        {
            var southWest = ParentMap.ViewToLocation(new Point(0d, ParentMap.ActualHeight));
            var northEast = ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth, 0d));

            var latLabelStart = Math.Ceiling(southWest.Latitude / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(southWest.Longitude / lineDistance) * lineDistance;

            for (var lat = latLabelStart; lat <= northEast.Latitude; lat += lineDistance)
            {
                var p1 = ParentMap.LocationToView(lat, southWest.Longitude);
                var p2 = ParentMap.LocationToView(lat, northEast.Longitude);
                figures.Add(CreateLineFigure(p1, p2));
            }

            for (var lon = lonLabelStart; lon <= northEast.Longitude; lon += lineDistance)
            {
                var p1 = ParentMap.LocationToView(southWest.Latitude, lon);
                var p2 = ParentMap.LocationToView(northEast.Latitude, lon);
                figures.Add(CreateLineFigure(p1, p2));

                for (var lat = latLabelStart; lat <= northEast.Latitude; lat += lineDistance)
                {
                    AddLabel(labels, labelFormat, lat, lon, ParentMap.LocationToView(lat, lon));
                }
            }
        }

        private void DrawGraticule(PathFigureCollection figures, List<Label> labels, double lineDistance, string labelFormat)
        {
            GetLatitudeRange(lineDistance, out double minLat, out double maxLat);

            var latSegments = (int)Math.Round(Math.Abs(maxLat - minLat) / lineDistance);
            var interpolationCount = Math.Max(1, (int)Math.Ceiling(lineDistance));
            var interpolationDistance = lineDistance / interpolationCount;
            var latPoints = latSegments * interpolationCount;
            var centerLon = Math.Round(ParentMap.Center.Longitude / lineDistance) * lineDistance;
            var minLon = centerLon - lineDistance;
            var maxLon = centerLon + lineDistance;

            if (DrawMeridian(figures, centerLon, minLat, interpolationDistance, latPoints))
            {
                while (DrawMeridian(figures, minLon, minLat, interpolationDistance, latPoints) &&
                    minLon > centerLon - 180d)
                {
                    minLon -= lineDistance;
                }

                while (DrawMeridian(figures, maxLon, minLat, interpolationDistance, latPoints) &&
                    maxLon < centerLon + 180d)
                {
                    maxLon += lineDistance;
                }
            }

            var lonSegments = (int)Math.Round(Math.Abs(maxLon - minLon) / lineDistance);

            for (var i = 1; i < latSegments; i++)
            {
                var lat = minLat + i * lineDistance;
                var lon = minLon;
                var points = new List<Point>();
                var mapPoint = ParentMap.MapProjection.LocationToMap(lat, lon);
                var position = ParentMap.ViewTransform.MapToView(mapPoint);
                var rotation = -ParentMap.MapProjection.GridConvergence(mapPoint.X, mapPoint.Y);

                points.Add(position);
                AddLabel(labels, labelFormat, lat, lon, position, rotation);

                for (int j = 0; j < lonSegments; j++)
                {
                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        lon = minLon + j * lineDistance + k * interpolationDistance;
                        mapPoint = ParentMap.MapProjection.LocationToMap(lat, lon);
                        position = ParentMap.ViewTransform.MapToView(mapPoint);
                        points.Add(position);
                    }

                    rotation = -ParentMap.MapProjection.GridConvergence(mapPoint.X, mapPoint.Y);
                    AddLabel(labels, labelFormat, lat, lon, position, rotation);
                }

                if (points.Count >= 2)
                {
                    figures.Add(CreatePolylineFigure(points));
                }
            }
        }

        private bool DrawMeridian(PathFigureCollection figures,
            double longitude, double startLatitude, double deltaLatitude, int numPoints)
        {
            var points = new List<Point>();
            var visible = false;

            for (int i = 0; i <= numPoints; i++)
            {
                var p = ParentMap.LocationToView(startLatitude + i * deltaLatitude, longitude);

                visible = visible ||
                    p.X >= 0d && p.X <= ParentMap.ActualWidth &&
                    p.Y >= 0d && p.Y <= ParentMap.ActualHeight;

                points.Add(p);
            }

            if (visible && points.Count >= 2)
            {
                figures.Add(CreatePolylineFigure(points));
            }

            return visible;
        }

        private void GetLatitudeRange(double lineDistance, out double minLatitude, out double maxLatitude)
        {
            var width = ParentMap.ActualWidth;
            var height = ParentMap.ActualHeight;
            var locations = new List<Location>
            {
                ParentMap.ViewToLocation(new Point(0d, 0d)),
                ParentMap.ViewToLocation(new Point(width, 0d)),
                ParentMap.ViewToLocation(new Point(0d, height)),
                ParentMap.ViewToLocation(new Point(width, height)),
                ParentMap.ViewToLocation(new Point(width / 2d, 0d)),
                ParentMap.ViewToLocation(new Point(width / 2d, height)),
                ParentMap.ViewToLocation(new Point(0d, height / 2d)),
                ParentMap.ViewToLocation(new Point(width, height / 2d)),
            };

            var pole = ParentMap.LocationToView(90d, 0d);
            if (pole.X >= 0d && pole.X <= width && pole.Y >= 0d && pole.Y <= height)
            {
                locations.Add(new Location(90d, 0d));
            }

            pole = ParentMap.LocationToView(-90d, 0d);
            if (pole.X >= 0d && pole.X <= width && pole.Y >= 0d && pole.Y <= height)
            {
                locations.Add(new Location(-90d, 0d));
            }

            var latitudes = locations.Select(loc => loc.Latitude).Distinct();
            var south = -90d;
            var north = 90d;

            if (latitudes.Any())
            {
                south = latitudes.Min();
                north = latitudes.Max();
            }

            minLatitude = Math.Max(Math.Floor(south / lineDistance - 1d) * lineDistance, -90d);
            maxLatitude = Math.Min(Math.Ceiling(north / lineDistance + 1d) * lineDistance, 90d);
        }

        private double GetLineDistance()
        {
            var pixelPerDegree = ParentMap.ViewTransform.Scale * MapProjection.Wgs84MeterPerDegree;

            if (!ParentMap.MapProjection.IsNormalCylindrical)
            {
                pixelPerDegree *= Math.Cos(ParentMap.Center.Latitude * Math.PI / 180d);
            }

            var minDistance = MinLineDistance / Math.Max(1d, pixelPerDegree);
            var scale = minDistance < 1d / 60d ? 3600d : minDistance < 1d ? 60d : 1d;
            minDistance *= scale;

            var lineDistances = new double[] { 1d, 2d, 5d, 10d, 15d, 30d, 60d };
            var i = 0;

            while (i < lineDistances.Length - 1 && lineDistances[i] < minDistance)
            {
                i++;
            }

            return Math.Min(lineDistances[i] / scale, 60d);
        }

        private void AddLabel(List<Label> labels, string labelFormat, double latitude, double longitude, Point position, double rotation = 0d)
        {
            if (position.X >= 0d && position.X <= ParentMap.ActualWidth &&
                position.Y >= 0d && position.Y <= ParentMap.ActualHeight)
            {
                rotation = (rotation + ParentMap.ViewTransform.Rotation) % 360d;

                if (rotation < -90d)
                {
                    rotation += 180d;
                }
                else if (rotation > 90d)
                {
                    rotation -= 180d;
                }

                labels.Add(new Label(
                    GetLabelText(latitude, labelFormat, "NS"),
                    GetLabelText(Location.NormalizeLongitude(longitude), labelFormat, "EW"),
                    position.X, position.Y, rotation));
            }
        }

        private static string GetLabelText(double value, string labelFormat, string hemispheres)
        {
            var hemisphere = hemispheres[0];

            if (value < -1e-8) // ~1 mm
            {
                value = -value;
                hemisphere = hemispheres[1];
            }

            var seconds = (int)Math.Round(value * 3600d);

            return string.Format(CultureInfo.InvariantCulture,
                labelFormat, hemisphere, seconds / 3600, seconds / 60 % 60, seconds % 60);
        }

        private static PathFigure CreateLineFigure(Point p1, Point p2)
        {
            var figure = new PathFigure
            {
                StartPoint = p1,
                IsFilled = false
            };

            figure.Segments.Add(new LineSegment { Point = p2 });
            return figure;
        }
    }
}
