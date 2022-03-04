// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule
    {
        private const double LineInterpolationResolution = 2d;

        private class Label
        {
            public readonly double Position;
            public readonly FormattedText Text;

            public Label(double position, FormattedText text)
            {
                Position = position;
                Text = text;
            }
        }

        static MapGraticule()
        {
            StrokeThicknessProperty.OverrideMetadata(typeof(MapGraticule), new FrameworkPropertyMetadata(0.5));
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var projection = ParentMap?.MapProjection;

            if (projection != null)
            {
                if (projection.IsNormalCylindrical)
                {
                    DrawCylindricalGraticule(drawingContext);
                }
                else
                {
                    DrawGraticule(drawingContext);
                }
            }
        }

        private void DrawCylindricalGraticule(DrawingContext drawingContext)
        {
            var path = new PathGeometry();
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var lineDistance = GetLineDistance();
            var labelFormat = GetLabelFormat(lineDistance);

            var boundingBox = ParentMap.ViewRectToBoundingBox(new Rect(ParentMap.RenderSize));
            var latLabelStart = Math.Ceiling(boundingBox.South / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(boundingBox.West / lineDistance) * lineDistance;
            var latLabels = new List<Label>((int)((boundingBox.North - latLabelStart) / lineDistance) + 1);
            var lonLabels = new List<Label>((int)((boundingBox.East - lonLabelStart) / lineDistance) + 1);

            for (var lat = latLabelStart; lat <= boundingBox.North; lat += lineDistance)
            {
                latLabels.Add(new Label(lat, new FormattedText(
                    GetLabelText(lat, labelFormat, "NS"),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip)));

                var p1 = ParentMap.LocationToView(new Location(lat, boundingBox.West));
                var p2 = ParentMap.LocationToView(new Location(lat, boundingBox.East));

                if (MapProjection.IsValid(p1) && MapProjection.IsValid(p2))
                {
                    var figure = new PathFigure { StartPoint = p1 };
                    figure.Segments.Add(new LineSegment(p2, true));
                    path.Figures.Add(figure);
                }
            }

            for (var lon = lonLabelStart; lon <= boundingBox.East; lon += lineDistance)
            {
                lonLabels.Add(new Label(lon, new FormattedText(
                    GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW"),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip)));

                var p1 = ParentMap.LocationToView(new Location(boundingBox.South, lon));
                var p2 = ParentMap.LocationToView(new Location(boundingBox.North, lon));

                if (MapProjection.IsValid(p1) && MapProjection.IsValid(p2))
                {
                    var figure = new PathFigure { StartPoint = p1 };
                    figure.Segments.Add(new LineSegment(p2, true));
                    path.Figures.Add(figure);
                }
            }

            drawingContext.DrawGeometry(null, CreatePen(), path);

            foreach (var latLabel in latLabels)
            {
                foreach (var lonLabel in lonLabels)
                {
                    var position = ParentMap.LocationToView(new Location(latLabel.Position, lonLabel.Position));

                    if (MapProjection.IsValid(position))
                    {
                        DrawLabel(drawingContext, latLabel.Text, lonLabel.Text, position, ParentMap.ViewTransform.Rotation);
                    }
                }
            }
        }

        private void DrawGraticule(DrawingContext drawingContext)
        {
            var path = new PathGeometry();
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var lineDistance = GetLineDistance();
            var labelFormat = GetLabelFormat(lineDistance);

            var centerLon = Math.Floor(ParentMap.Center.Longitude / lineDistance) * lineDistance;
            var minLon = centerLon - lineDistance;
            var maxLon = centerLon + lineDistance;
            var minLat = 0d;
            var maxLat = 0d;

            GetLatitudeRange(lineDistance, ref minLat, ref maxLat);

            var latSegments = (int)Math.Round(Math.Abs(maxLat - minLat) / lineDistance);
            var interpolationCount = Math.Max(1, (int)Math.Ceiling(lineDistance / LineInterpolationResolution));
            var interpolationDistance = lineDistance / interpolationCount;
            var latPoints = latSegments * interpolationCount;

            if (DrawMeridian(path.Figures, centerLon, minLat, interpolationDistance, latPoints))
            {
                while (minLon > centerLon - 180d &&
                    DrawMeridian(path.Figures, minLon, minLat, interpolationDistance, latPoints))
                {
                    minLon -= lineDistance;
                }

                while (maxLon <= centerLon + 180d &&
                    DrawMeridian(path.Figures, maxLon, minLat, interpolationDistance, latPoints))
                {
                    maxLon += lineDistance;
                }
            }

            var lonSegments = (int)Math.Round(Math.Abs(maxLon - minLon) / lineDistance);

            for (var s = minLat > -90d ? 0 : 1; s < latSegments; s++)
            {
                var lat = minLat + s * lineDistance;
                var lon = minLon;
                var points = new List<Point>();
                var p = ParentMap.LocationToView(new Location(lat, lon));

                if (MapProjection.IsValid(p))
                {
                    points.Add(p);
                }

                for (int i = 0; i < lonSegments; i++)
                {
                    for (int j = 1; j <= interpolationCount; j++)
                    {
                        lon = minLon + i * lineDistance + j * interpolationDistance;
                        p = ParentMap.LocationToView(new Location(lat, lon));

                        if (MapProjection.IsValid(p))
                        {
                            points.Add(p);
                        }
                    }

                    if (p.X >= 0d && p.X <= ParentMap.RenderSize.Width &&
                        p.Y >= 0d && p.Y <= ParentMap.RenderSize.Height)
                    {
                        DrawLabel(drawingContext, typeface, pixelsPerDip, p, new Location(lat, lon), labelFormat);
                    }
                }

                if (points.Count >= 2)
                {
                    var figure = new PathFigure { StartPoint = points.First() };
                    figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
                    path.Figures.Add(figure);
                }
            }

            drawingContext.DrawGeometry(null, CreatePen(), path);
        }

        private bool DrawMeridian(PathFigureCollection figures,
            double longitude, double startLatitude, double deltaLatitude, int numPoints)
        {
            var points = new List<Point>();
            var visible = false;

            for (int i = 0; i <= numPoints; i++)
            {
                var p = ParentMap.LocationToView(new Location(startLatitude + i * deltaLatitude, longitude));

                if (MapProjection.IsValid(p))
                {
                    visible = visible ||
                        p.X >= 0d && p.X <= ParentMap.RenderSize.Width &&
                        p.Y >= 0d && p.Y <= ParentMap.RenderSize.Height;

                    points.Add(p);
                }
            }

            if (points.Count >= 2)
            {
                var figure = new PathFigure { StartPoint = points.First() };
                figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
                figures.Add(figure);
            }

            return visible;
        }

        private void DrawLabel(DrawingContext drawingContext, Typeface typeface, double pixelsPerDip,
            Point position, Location location, string labelFormat)
        {
            var latText = new FormattedText(GetLabelText(location.Latitude, labelFormat, "NS"),
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);
            var lonText = new FormattedText(GetLabelText(location.Longitude, labelFormat, "EW"),
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip);

            location.Longitude += latText.Width / PixelPerLongitudeDegree(location);

            var p = ParentMap.LocationToView(location);

            if (MapProjection.IsValid(p))
            {
                DrawLabel(drawingContext, latText, lonText, position, Vector.AngleBetween(new Vector(1d, 0d), p - position));
            }
        }

        private void DrawLabel(DrawingContext drawingContext,
            FormattedText latitudeLabel, FormattedText longitudeLabel, Point position, double rotation)
        {
            var x = position.X + StrokeThickness / 2d + 2d;
            var y1 = position.Y - StrokeThickness / 2d - latitudeLabel.Height;
            var y2 = position.Y + StrokeThickness / 2d;

            drawingContext.PushTransform(new RotateTransform(rotation, position.X, position.Y));
            drawingContext.DrawText(latitudeLabel, new Point(x, y1));
            drawingContext.DrawText(longitudeLabel, new Point(x, y2));
            drawingContext.Pop();
        }

        private void GetLatitudeRange(double lineDistance, ref double minLatitude, ref double maxLatitude)
        {
            var width = ParentMap.RenderSize.Width;
            var height = ParentMap.RenderSize.Height;
            var northPole = ParentMap.LocationToView(new Location(90d, 0d));
            var southPole = ParentMap.LocationToView(new Location(-90d, 0d));

            if (northPole.X >= 0d && northPole.Y >= 0d && northPole.X <= width && northPole.Y <= height)
            {
                maxLatitude = 90d;
            }

            if (southPole.X >= 0d && southPole.Y >= 0d && southPole.X <= width && southPole.Y <= height)
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
    }
}
