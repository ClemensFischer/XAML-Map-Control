// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule : MapOverlay
    {
        private class LonLabel
        {
            public readonly double Longitude;
            public readonly string Text;

            public LonLabel(double longitude, string text)
            {
                Longitude = longitude;
                Text = text;
            }
        }

        private class LatLabel
        {
            public readonly double TransformedLatitude;
            public readonly double Latitude;
            public readonly string Text;

            public LatLabel(double transformedLatitude, double latitude, string text)
            {
                TransformedLatitude = transformedLatitude;
                Latitude = latitude;
                Text = text;
            }
        }

        private Dictionary<string, GlyphRun> glyphRuns = new Dictionary<string, GlyphRun>();

        static MapGraticule()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapGraticule), new FrameworkPropertyMetadata(false));

            MapOverlay.StrokeThicknessProperty.OverrideMetadata(
                typeof(MapGraticule), new FrameworkPropertyMetadata(0.5, (o, e) => ((MapGraticule)o).glyphRuns.Clear()));
        }

        protected override void OnViewportChanged()
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var bounds = ParentMap.ViewportTransform.Inverse.TransformBounds(new Rect(ParentMap.RenderSize));
                var startPoint = new Point(bounds.X, bounds.Y);
                var endPoint = new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height);
                var startLocation = ParentMap.MapTransform.Transform(startPoint);
                var endLocation = ParentMap.MapTransform.Transform(endPoint);
                var minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, ParentMap.ZoomLevel) * TileSource.TileSize);
                var spacing = LineSpacings[LineSpacings.Length - 1];

                if (spacing >= minSpacing)
                {
                    spacing = LineSpacings.FirstOrDefault(s => s >= minSpacing);
                }

                var labelFormat = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";
                var labelStart = new Location(
                    Math.Ceiling(startLocation.Latitude / spacing) * spacing,
                    Math.Ceiling(startLocation.Longitude / spacing) * spacing);

                var latLabels = new List<LatLabel>((int)((endLocation.Latitude - labelStart.Latitude) / spacing) + 1);
                var lonLabels = new List<LonLabel>((int)((endLocation.Longitude - labelStart.Longitude) / spacing) + 1);

                for (var lat = labelStart.Latitude; lat <= endLocation.Latitude; lat += spacing)
                {
                    var location = new Location(lat, startLocation.Longitude);
                    var p1 = ParentMap.LocationToViewportPoint(location);
                    location.Longitude = endLocation.Longitude;
                    var p2 = ParentMap.LocationToViewportPoint(location);

                    latLabels.Add(new LatLabel(location.TransformedLatitude, lat, CoordinateString(lat, labelFormat, "NS")));

                    drawingContext.DrawLine(Pen, p1, p2);
                }

                for (var lon = labelStart.Longitude; lon <= endLocation.Longitude; lon += spacing)
                {
                    lonLabels.Add(new LonLabel(lon, CoordinateString(Location.NormalizeLongitude(lon), labelFormat, "EW")));

                    drawingContext.DrawLine(Pen,
                        ParentMap.LocationToViewportPoint(new Location(startPoint.Y, startLocation.Latitude, lon)),
                        ParentMap.LocationToViewportPoint(new Location(endPoint.Y, endLocation.Latitude, lon)));
                }

                if (Foreground != null && Foreground != Brushes.Transparent && latLabels.Count > 0 && lonLabels.Count > 0)
                {
                    var latLabelOrigin = new Point(StrokeThickness / 2d + 2d, -StrokeThickness / 2d - FontSize / 4d);
                    var lonLabelOrigin = new Point(StrokeThickness / 2d + 2d, StrokeThickness / 2d + FontSize);
                    var transform = Matrix.Identity;
                    transform.Rotate(ParentMap.Heading);

                    foreach (var latLabel in latLabels)
                    {
                        foreach (var lonLabel in lonLabels)
                        {
                            GlyphRun latGlyphRun;
                            GlyphRun lonGlyphRun;

                            if (!glyphRuns.TryGetValue(latLabel.Text, out latGlyphRun))
                            {
                                latGlyphRun = GlyphRunText.Create(latLabel.Text, Typeface, FontSize, latLabelOrigin);
                                glyphRuns.Add(latLabel.Text, latGlyphRun);
                            }

                            if (!glyphRuns.TryGetValue(lonLabel.Text, out lonGlyphRun))
                            {
                                lonGlyphRun = GlyphRunText.Create(lonLabel.Text, Typeface, FontSize, lonLabelOrigin);
                                glyphRuns.Add(lonLabel.Text, lonGlyphRun);
                            }

                            var position = ParentMap.LocationToViewportPoint(new Location(latLabel.TransformedLatitude, latLabel.Latitude, lonLabel.Longitude));

                            drawingContext.PushTransform(new MatrixTransform(
                                transform.M11, transform.M12, transform.M21, transform.M22, position.X, position.Y));

                            drawingContext.DrawGlyphRun(Foreground, latGlyphRun);
                            drawingContext.DrawGlyphRun(Foreground, lonGlyphRun);
                            drawingContext.Pop();
                        }
                    }

                    var removeKeys = glyphRuns.Keys.Where(k => !latLabels.Any(l => l.Text == k) && !lonLabels.Any(l => l.Text == k));

                    foreach (var key in removeKeys.ToList())
                    {
                        glyphRuns.Remove(key);
                    }
                }
                else
                {
                    glyphRuns.Clear();
                }
            }
        }
    }
}
