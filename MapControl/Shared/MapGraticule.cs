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

        private const double LineInterpolationResolution = 2d;

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

        private double PixelPerDegree => Math.Max(1d, ParentMap.ViewTransform.Scale * MapProjection.Wgs84MeterPerDegree);

        private double lineDistance;
        private string labelFormat;

        private void SetLineDistance()
        {
            var minDistance = MinLineDistance / PixelPerDegree;
            var scale = minDistance < 1d / 60d ? 3600d : minDistance < 1d ? 60d : 1d;
            minDistance *= scale;

            var lineDistances = new double[] { 1d, 2d, 5d, 10d, 15d, 30d, 60d };
            var i = 0;

            while (i < lineDistances.Length - 1 && lineDistances[i] < minDistance)
            {
                i++;
            }

            lineDistance = Math.Min(lineDistances[i] / scale, 30d);

            labelFormat = lineDistance < 1d / 60d ? "{0} {1}°{2:00}'{3:00}\""
                        : lineDistance < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";
        }

        private string GetLabelText(double value, string hemispheres)
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

        private void AddLabel(List<Label> labels, double latitude, double longitude, Point position, double? rotation = null)
        {
            if (position.X >= 0d && position.X <= ParentMap.ActualWidth &&
                position.Y >= 0d && position.Y <= ParentMap.ActualHeight)
            {
                if (!rotation.HasValue)
                {
                    // Get rotation from second location with same latitude.
                    //
                    var pos = ParentMap.LocationToView(latitude, longitude + 10d / PixelPerDegree);

                    rotation = Math.Atan2(pos.Y - position.Y, pos.X - position.X) * 180d / Math.PI;
                }

                if (rotation.HasValue)
                {
                    labels.Add(new Label(
                        GetLabelText(latitude, "NS"),
                        GetLabelText(Location.NormalizeLongitude(longitude), "EW"),
                        position.X, position.Y, rotation.Value));
                }
            }
        }

        private List<Label> DrawGraticule(PathFigureCollection figures)
        {
            var labels = new List<Label>();

            figures.Clear();

            SetLineDistance();

            if (ParentMap.MapProjection.IsNormalCylindrical)
            {
                DrawCylindricalGraticule(figures, labels);
            }
            else
            {
                DrawGraticule(figures, labels);
            }

            return labels;
        }

        private void DrawCylindricalGraticule(PathFigureCollection figures, List<Label> labels)
        {
            var boundingBox = ParentMap.ViewToBoundingBox(new Rect(0d, 0d, ParentMap.ActualWidth, ParentMap.ActualHeight));
            var latLabelStart = Math.Ceiling(boundingBox.South / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(boundingBox.West / lineDistance) * lineDistance;

            for (var lat = latLabelStart; lat <= boundingBox.North; lat += lineDistance)
            {
                var p1 = ParentMap.LocationToView(lat, boundingBox.West);
                var p2 = ParentMap.LocationToView(lat, boundingBox.East);
                figures.Add(CreateLineFigure(p1, p2));
            }

            for (var lon = lonLabelStart; lon <= boundingBox.East; lon += lineDistance)
            {
                var p1 = ParentMap.LocationToView(boundingBox.South, lon);
                var p2 = ParentMap.LocationToView(boundingBox.North, lon);
                figures.Add(CreateLineFigure(p1, p2));

                for (var lat = latLabelStart; lat <= boundingBox.North; lat += lineDistance)
                {
                    var position = ParentMap.LocationToView(lat, lon);
                    AddLabel(labels, lat, lon, position, ParentMap.ViewTransform.Rotation);
                }
            }
        }

        private void DrawGraticule(PathFigureCollection figures, List<Label> labels)
        {
            GetLatitudeRange(lineDistance, out double minLat, out double maxLat);

            var latSegments = (int)Math.Round(Math.Abs(maxLat - minLat) / lineDistance);
            var interpolationCount = Math.Max(1, (int)Math.Ceiling(lineDistance / LineInterpolationResolution));
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
                var position = ParentMap.LocationToView(lat, lon);

                points.Add(position);
                AddLabel(labels, lat, lon, position);

                for (int j = 0; j < lonSegments; j++)
                {
                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        lon = minLon + j * lineDistance + k * interpolationDistance;
                        position = ParentMap.LocationToView(lat, lon);
                        points.Add(position);
                    }

                    AddLabel(labels, lat, lon, position);
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

            if (points.Count >= 2)
            {
                figures.Add(CreatePolylineFigure(points));
            }

            return visible;
        }

        private void GetLatitudeRange(double lineDistance, out double minLatitude, out double maxLatitude)
        {
            var width = ParentMap.ActualWidth;
            var height = ParentMap.ActualHeight;
            var locations = new Location[]
            {
                ParentMap.ViewToLocation(new Point(0d, 0d)),
                ParentMap.ViewToLocation(new Point(width / 2d, 0d)),
                ParentMap.ViewToLocation(new Point(width, 0d)),
                ParentMap.ViewToLocation(new Point(width, height / 2d)),
                ParentMap.ViewToLocation(new Point(width, height)),
                ParentMap.ViewToLocation(new Point(width / 2d, height)),
                ParentMap.ViewToLocation(new Point(0d, height)),
                ParentMap.ViewToLocation(new Point(0d, height / 2)),
            };

            var latitudes = locations.Where(loc => loc != null).Select(loc => loc.Latitude).Distinct();
            var south = -90d;
            var north = 90d;

            if (latitudes.Any())
            {
                south = latitudes.Min();
                north = latitudes.Max();
            }

            minLatitude = Math.Max(Math.Floor(south / lineDistance) * lineDistance, -90d);
            maxLatitude = Math.Min(Math.Ceiling(north / lineDistance) * lineDistance, 90d);
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
