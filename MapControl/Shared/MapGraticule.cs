// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
#endif

namespace MapControl
{
    /// <summary>
    /// Draws a graticule overlay.
    /// </summary>
    public partial class MapGraticule
    {
        private class Label
        {
            public Label(string latText, string lonText, double x, double y, double rotation)
            {
                LatitudeText = latText;
                LongitudeText = lonText;
                X = x;
                Y = y;
                Rotation = rotation;
            }

            public string LatitudeText { get; }
            public string LongitudeText { get; }
            public double X { get; }
            public double Y { get; }
            public double Rotation { get; }
        }

        private const double LineInterpolationResolution = 2d;

        public static readonly DependencyProperty MinLineDistanceProperty =
            DependencyPropertyHelper.Register<MapGraticule, double>(nameof(MinLineDistance), 150d);

        private double lineDistance;
        private string labelFormat;

        /// <summary>
        /// Minimum graticule line distance in pixels. The default value is 150.
        /// </summary>
        public double MinLineDistance
        {
            get => (double)GetValue(MinLineDistanceProperty);
            set => SetValue(MinLineDistanceProperty, value);
        }

        private void SetLineDistance()
        {
            var minDistance = MinLineDistance / PixelPerLongitudeDegree(ParentMap.Center);
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

        private double PixelPerLongitudeDegree(Location location)
        {
            return Math.Max(1d, // a reasonable lower limit
                ParentMap.GetScale(location).X *
                Math.Cos(location.Latitude * Math.PI / 180d) * MapProjection.Wgs84MeterPerDegree);
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

        private void AddLabel(ICollection<Label> labels, Location location, Point position, double? rotation = null)
        {
            if (position.X >= 0d && position.X <= ParentMap.RenderSize.Width &&
                position.Y >= 0d && position.Y <= ParentMap.RenderSize.Height)
            {
                if (!rotation.HasValue)
                {
                    // Get rotation from second location with same latitude.
                    //
                    var pos = ParentMap.LocationToView(
                        new Location(location.Latitude, location.Longitude + 10d / PixelPerLongitudeDegree(location)));

                    if (pos.HasValue)
                    {
                        rotation = Math.Atan2(pos.Value.Y - position.Y, pos.Value.X - position.X) * 180d / Math.PI;
                    }
                }

                if (rotation.HasValue)
                {
                    labels.Add(new Label(
                        GetLabelText(location.Latitude, "NS"),
                        GetLabelText(Location.NormalizeLongitude(location.Longitude), "EW"),
                        position.X, position.Y, rotation.Value));
                }
            }
        }

        private ICollection<Label> DrawGraticule(PathFigureCollection figures)
        {
            var labels = new List<Label>();

            figures.Clear();

            SetLineDistance();

            if (ParentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical)
            {
                DrawCylindricalGraticule(figures, labels);
            }
            else
            {
                DrawGraticule(figures, labels);
            }

            return labels;
        }

        private void DrawCylindricalGraticule(PathFigureCollection figures, ICollection<Label> labels)
        {
            var bounds = ParentMap.ViewRectToBoundingBox(new Rect(0, 0, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height));
            var latLabelStart = Math.Ceiling(bounds.South / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(bounds.West / lineDistance) * lineDistance;

            for (var lat = latLabelStart; lat <= bounds.North; lat += lineDistance)
            {
                var p1 = ParentMap.LocationToView(new Location(lat, bounds.West));
                var p2 = ParentMap.LocationToView(new Location(lat, bounds.East));

                if (p1.HasValue && p2.HasValue)
                {
                    figures.Add(CreateLineFigure(p1.Value, p2.Value));
                }
            }

            for (var lon = lonLabelStart; lon <= bounds.East; lon += lineDistance)
            {
                var p1 = ParentMap.LocationToView(new Location(bounds.South, lon));
                var p2 = ParentMap.LocationToView(new Location(bounds.North, lon));

                if (p1.HasValue && p2.HasValue)
                {
                    figures.Add(CreateLineFigure(p1.Value, p2.Value));
                }

                for (var lat = latLabelStart; lat <= bounds.North; lat += lineDistance)
                {
                    var location = new Location(lat, lon);
                    var position = ParentMap.LocationToView(location);

                    if (position.HasValue)
                    {
                        AddLabel(labels, location, position.Value, ParentMap.ViewTransform.Rotation);
                    }
                }
            }
        }

        private void DrawGraticule(PathFigureCollection figures, ICollection<Label> labels)
        {
            var minLat = 0d;
            var maxLat = 0d;

            GetLatitudeRange(lineDistance, ref minLat, ref maxLat);

            var latSegments = (int)Math.Round(Math.Abs(maxLat - minLat) / lineDistance);
            var interpolationCount = Math.Max(1, (int)Math.Ceiling(lineDistance / LineInterpolationResolution));
            var interpolationDistance = lineDistance / interpolationCount;
            var latPoints = latSegments * interpolationCount;

            var centerLon = Math.Round(ParentMap.Center.Longitude / lineDistance) * lineDistance;
            var westLimit = centerLon - 180d;
            var eastLimit = centerLon + 180d;

            if (ParentMap.MapProjection.Type == MapProjectionType.TransverseCylindrical)
            {
                westLimit = ParentMap.MapProjection.Center.Longitude - 15d;
                eastLimit = ParentMap.MapProjection.Center.Longitude + 15d;
                westLimit = Math.Floor(westLimit / lineDistance) * lineDistance;
                eastLimit = Math.Ceiling(eastLimit / lineDistance) * lineDistance;
            }

            var minLon = centerLon - lineDistance;
            var maxLon = centerLon + lineDistance;

            if (DrawMeridian(figures, centerLon, minLat, interpolationDistance, latPoints))
            {
                while (DrawMeridian(figures, minLon, minLat, interpolationDistance, latPoints) &&
                    minLon > westLimit)
                {
                    minLon -= lineDistance;
                }

                while (DrawMeridian(figures, maxLon, minLat, interpolationDistance, latPoints) &&
                    maxLon < eastLimit)
                {
                    maxLon += lineDistance;
                }
            }

            var lonSegments = (int)Math.Round(Math.Abs(maxLon - minLon) / lineDistance);

            for (var i = 1; i < latSegments; i++)
            {
                var lat = minLat + i * lineDistance;
                var lon = minLon;
                var location = new Location(lat, lon);
                var points = new List<Point>();
                var position = ParentMap.LocationToView(location);

                if (position.HasValue)
                {
                    points.Add(position.Value);
                    AddLabel(labels, location, position.Value);
                }

                for (int j = 0; j < lonSegments; j++)
                {
                    for (int k = 1; k <= interpolationCount; k++)
                    {
                        lon = minLon + j * lineDistance + k * interpolationDistance;
                        location = new Location(lat, lon);
                        position = ParentMap.LocationToView(location);

                        if (position.HasValue)
                        {
                            points.Add(position.Value);
                        }
                    }

                    if (position.HasValue)
                    {
                        AddLabel(labels, location, position.Value);
                    }
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
                var p = ParentMap.LocationToView(new Location(startLatitude + i * deltaLatitude, longitude));

                if (p.HasValue)
                {
                    visible = visible ||
                        p.Value.X >= 0d && p.Value.X <= ParentMap.RenderSize.Width &&
                        p.Value.Y >= 0d && p.Value.Y <= ParentMap.RenderSize.Height;

                    points.Add(p.Value);
                }
            }

            if (points.Count >= 2)
            {
                figures.Add(CreatePolylineFigure(points));
            }

            return visible;
        }

        private void GetLatitudeRange(double lineDistance, ref double minLatitude, ref double maxLatitude)
        {
            var width = ParentMap.RenderSize.Width;
            var height = ParentMap.RenderSize.Height;
            var northPole = ParentMap.LocationToView(new Location(90d, 0d));
            var southPole = ParentMap.LocationToView(new Location(-90d, 0d));

            if (northPole.HasValue &&
                northPole.Value.X >= 0d && northPole.Value.X <= width &&
                northPole.Value.Y >= 0d && northPole.Value.Y <= height)
            {
                maxLatitude = 90d;
            }

            if (southPole.HasValue &&
                southPole.Value.X >= 0d && southPole.Value.X <= width &&
                southPole.Value.Y >= 0d && southPole.Value.Y <= height)
            {
                minLatitude = -90d;
            }

            if (minLatitude > -90d || maxLatitude < 90d)
            {
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

                var latitudes = locations.Where(loc => loc != null).Select(loc => loc.Latitude);
                var south = -90d;
                var north = 90d;

                if (latitudes.Distinct().Count() >= 2)
                {
                    south = latitudes.Min();
                    north = latitudes.Max();
                }

                if (minLatitude > -90d)
                {
                    minLatitude = Math.Max(Math.Floor(south / lineDistance) * lineDistance, -90d);
                }

                if (maxLatitude < 90d)
                {
                    maxLatitude = Math.Min(Math.Ceiling(north / lineDistance) * lineDistance, 90d);
                }
            }
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
