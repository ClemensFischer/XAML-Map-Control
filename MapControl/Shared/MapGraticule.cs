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

        private List<Label> DrawGraticule(PathFigureCollection figures)
        {
            var labels = new List<Label>();

            figures.Clear();

            if (ParentMap.MapProjection.IsNormalCylindrical)
            {
                DrawNormalCylindrical(figures, labels);
            }
            else
            {
                DrawGraticule(figures, labels);
            }

            return labels;
        }

        private void DrawNormalCylindrical(PathFigureCollection figures, List<Label> labels)
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

        private void DrawGraticule(PathFigureCollection figures, List<Label> labels)
        {
            var lineDistance = GetLineDistance(true);
            var labelFormat = GetLabelFormat(lineDistance);

            GetLatitudeRange(lineDistance, out double minLat, out double maxLat);

            var latSegments = (int)Math.Ceiling(Math.Abs(maxLat - minLat) / lineDistance);
            var interpolationCount = Math.Max(1, (int)Math.Ceiling(lineDistance));
            var interpolationStep = lineDistance / interpolationCount;

            var latStep = lineDistance;
            var startLat = minLat;

            if (Math.Abs(minLat) > Math.Abs(maxLat))
            {
                startLat = maxLat;
                latStep = -lineDistance;
            }

            var centerLon = Math.Round(ParentMap.Center.Longitude / lineDistance) * lineDistance;
            var minLon = centerLon;
            var maxLon = centerLon + lineDistance;

            while (DrawMeridian(figures, minLon, minLat, interpolationStep, latSegments * interpolationCount) &&
                minLon > centerLon - 180d)
            {
                minLon -= lineDistance;
            }

            while (DrawMeridian(figures, maxLon, minLat, interpolationStep, latSegments * interpolationCount) &&
                maxLon < centerLon + 180d)
            {
                maxLon += lineDistance;
            }

            var lonSegments = (int)Math.Round(Math.Abs(maxLon - minLon) / lineDistance);

            for (var i = 1; i < latSegments; i++)
            {
                var lat = startLat + i * latStep;
                var lon = minLon;
                var points = new List<Point>();
                var position = ParentMap.LocationToView(lat, lon);
                var rotation = -ParentMap.MapProjection.GridConvergence(lat, lon);

                points.Add(position);
                AddLabel(labels, labelFormat, lat, lon, position, rotation);

                for (int j = 0; j < lonSegments; j++)
                {
                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        lon = minLon + j * lineDistance + k * interpolationStep;
                        position = ParentMap.LocationToView(lat, lon);
                        points.Add(position);
                    }

                    rotation = -ParentMap.MapProjection.GridConvergence(lat, lon);
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
                points.Add(p);
                visible = visible || ParentMap.InsideViewBounds(p);
            }

            if (visible && points.Count >= 2)
            {
                figures.Add(CreatePolylineFigure(points));
            }

            return visible;
        }

        private void GetLatitudeRange(double lineDistance, out double minLatitude, out double maxLatitude)
        {
            var minLat = 90d;
            var maxLat = -90d;

            if (ParentMap.InsideViewBounds(ParentMap.LocationToView(90d, 0d)))
            {
                maxLat = 90d;
            }

            if (ParentMap.InsideViewBounds(ParentMap.LocationToView(-90d, 0d)))
            {
                minLat = -90d;
            }

            if (minLat > -90d || maxLat < 90d)
            {
                var locations = new Location[]
                {
                    ParentMap.ViewToLocation(new Point(0d, 0d)),
                    ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth, 0d)),
                    ParentMap.ViewToLocation(new Point(0d, ParentMap.ActualHeight)),
                    ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth, ParentMap.ActualHeight)),
                    ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth / 2d, 0d)),
                    ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth / 2d, ParentMap.ActualHeight)),
                    ParentMap.ViewToLocation(new Point(0d, ParentMap.ActualHeight / 2d)),
                    ParentMap.ViewToLocation(new Point(ParentMap.ActualWidth, ParentMap.ActualHeight / 2d)),
                };

                var latitudes = locations.Select(loc => loc.Latitude).Distinct();
                minLat = Math.Min(minLat, latitudes.Min());
                maxLat = Math.Max(maxLat, latitudes.Max());
            }

            minLatitude = Math.Max(Math.Floor(minLat / lineDistance) * lineDistance, -90d);
            maxLatitude = Math.Min(Math.Ceiling(maxLat / lineDistance) * lineDistance, 90d);
        }

        private void AddLabel(List<Label> labels, string labelFormat, double latitude, double longitude, Point position, double rotation = 0d)
        {
            if (ParentMap.InsideViewBounds(position))
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
