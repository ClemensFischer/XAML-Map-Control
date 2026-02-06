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

        private static readonly double[] lineDistances = [
            1d/3600d, 1d/1800d, 1d/720d, 1d/360d, 1d/240d, 1d/120d,
            1d/60d, 1d/30d, 1d/12d, 1d/6d, 1d/4d, 1d/2d,
            1d, 2d, 5d, 10d, 15d, 30d];


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

        private void DrawGraticule(PathFigureCollection figures, List<Label> labels)
        {
            if (ParentMap.MapProjection.IsNormalCylindrical)
            {
                DrawNormalGraticule(figures, labels);
            }
            else
            {
                DrawNonNormalGraticule(figures, labels);
            }
        }

        private void DrawNormalGraticule(PathFigureCollection figures, List<Label> labels)
        {
            var southWest = ParentMap.ViewToLocation(new Point(0d, ParentMap.ActualHeight));
            var northEast = ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth, 0d));
            var lineDistance = GetLineDistance(false);
            var latLabelStart = Math.Ceiling(southWest.Latitude / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(southWest.Longitude / lineDistance) * lineDistance;
            var labelFormat = GetLabelFormat(lineDistance);

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

        private void DrawNonNormalGraticule(PathFigureCollection figures, List<Label> labels)
        {
            var lineDistance = GetLineDistance(true);
            var labelFormat = GetLabelFormat(lineDistance);
            var pointDistance = Math.Min(lineDistance, 1d);
            var interpolationCount = (int)(lineDistance / pointDistance);
            var centerLat = Math.Round(ParentMap.Center.Latitude / pointDistance) * pointDistance;
            var centerLon = Math.Round(ParentMap.Center.Longitude / lineDistance) * lineDistance;
            var minLat = centerLat;
            var maxLat = centerLat;
            var minLon = centerLon;
            var maxLon = centerLon;

            for (var lon = centerLon;
                lon >= centerLon - 180d && DrawMeridian(figures, lon, pointDistance, ref minLat, ref maxLat);
                lon -= lineDistance)
            {
                minLon = lon;
            }

            for (var lon = centerLon + lineDistance;
                lon < centerLon + 180d && DrawMeridian(figures, lon, pointDistance, ref minLat, ref maxLat);
                lon += lineDistance)
            {
                maxLon = lon;
            }

            minLat = Math.Ceiling(minLat / lineDistance) * lineDistance;
            maxLat = Math.Floor(maxLat / lineDistance) * lineDistance;

            if (minLon + 360d > maxLon)
            {
                minLon -= lineDistance;
            }

            if (maxLon - 360d < minLon)
            {
                maxLon += lineDistance;
            }

            var lonSegments = (int)((maxLon - minLon) / lineDistance);

            for (var lat = minLat; lat <= maxLat; lat += lineDistance)
            {
                var points = new List<Point>();

                for (int i = 0; i <= lonSegments; i++)
                {
                    var lon = minLon + i * lineDistance;
                    var p = ParentMap.LocationToView(lat, lon);
                    points.Add(p);
                    AddLabel(labels, labelFormat, lat, lon, p);

                    for (int j = 1; j < interpolationCount; j++)
                    {
                        points.Add(ParentMap.LocationToView(lat, lon + j * pointDistance));
                    }
                }

                if (points.Count >= 2)
                {
                    figures.Add(CreatePolylineFigure(points));
                }
            }
        }

        private bool DrawMeridian(PathFigureCollection figures, double lon,
            double latStep, ref double minLat, ref double maxLat)
        {
            var points = new List<Point>();
            var visible = false;

            for (var lat = minLat + latStep; lat < maxLat; lat += latStep)
            {
                var p = ParentMap.LocationToView(lat, lon);
                points.Add(p);
                visible = visible || ParentMap.InsideViewBounds(p);
            }

            for (var lat = minLat; lat >= -90d; lat -= latStep)
            {
                var p = ParentMap.LocationToView(lat, lon);
                points.Insert(0, p);
                if (!ParentMap.InsideViewBounds(p)) break;
                minLat = lat;
                visible = true;
            }

            for (var lat = maxLat; lat <= 90d; lat += latStep)
            {
                var p = ParentMap.LocationToView(lat, lon);
                points.Add(p);
                if (!ParentMap.InsideViewBounds(p)) break;
                maxLat = lat;
                visible = true;
            }

            if (visible && points.Count >= 2)
            {
                figures.Add(CreatePolylineFigure(points));
            }

            return visible;
        }

        private void AddLabel(List<Label> labels, string labelFormat, double lat, double lon, Point position)
        {
            if (lat > -90d && lat < 90d && ParentMap.InsideViewBounds(position))
            {
                var rotation = (ParentMap.ViewTransform.Rotation - ParentMap.MapProjection.GridConvergence(lat, lon)) % 360d;

                if (rotation < -90d)
                {
                    rotation += 180d;
                }
                else if (rotation > 90d)
                {
                    rotation -= 180d;
                }

                labels.Add(new Label(
                    GetLabelText(lat, labelFormat, "NS"),
                    GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW"),
                    position.X, position.Y, rotation));
            }
        }

        private double GetLineDistance(bool scaleByLatitude)
        {
            var minDistance = MinLineDistance / (ParentMap.ViewTransform.Scale * MapProjection.Wgs84MeterPerDegree);

            if (scaleByLatitude)
            {
                minDistance /= Math.Cos(ParentMap.Center.Latitude * Math.PI / 180d);
            }

            minDistance = Math.Max(minDistance, lineDistances.First());
            minDistance = Math.Min(minDistance, lineDistances.Last());

            return lineDistances.First(d => d >= minDistance);
        }

        private static string GetLabelFormat(double lineDistance)
        {
            return lineDistance < 1d / 60d ? "{0} {1}°{2:00}'{3:00}\"" :
                   lineDistance < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";
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

        private static PathFigure CreatePolylineFigure(IEnumerable<Point> points)
        {
            var figure = new PathFigure
            {
                StartPoint = points.First(),
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(CreatePolyLineSegment(points.Skip(1)));
            return figure;
        }
    }
}
