// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            var parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null)
            {
                var bounds = parentMap.ViewportTransform.Inverse.TransformBounds(new Rect(parentMap.RenderSize));
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

                for (var lat = labelsStart.Latitude; lat <= end.Latitude; lat += spacing)
                {
                    drawingContext.DrawLine(Pen,
                        parentMap.LocationToViewportPoint(new Location(lat, start.Longitude)),
                        parentMap.LocationToViewportPoint(new Location(lat, end.Longitude)));
                }

                for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                {
                    drawingContext.DrawLine(Pen,
                        parentMap.LocationToViewportPoint(new Location(start.Latitude, lon)),
                        parentMap.LocationToViewportPoint(new Location(end.Latitude, lon)));
                }

                if (Foreground != null && Foreground != Brushes.Transparent)
                {
                    var format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

                    for (var lat = labelsStart.Latitude; lat <= end.Latitude; lat += spacing)
                    {
                        for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                        {
                            var t = StrokeThickness / 2d;
                            var p = parentMap.LocationToViewportPoint(new Location(lat, lon));
                            var latPos = new Point(p.X + t + 2d, p.Y - t - FontSize / 4d);
                            var lonPos = new Point(p.X + t + 2d, p.Y + t + FontSize);
                            var latLabel = CoordinateString(lat, format, "NS");
                            var lonLabel = CoordinateString(Location.NormalizeLongitude(lon), format, "EW");

                            drawingContext.PushTransform(new RotateTransform(parentMap.Heading, p.X, p.Y));
                            drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(latLabel, Typeface, FontSize, latPos));
                            drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(lonLabel, Typeface, FontSize, lonPos));
                            drawingContext.Pop();
                        }
                    }
                }
            }
        }
    }
}
