// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule : MapOverlay
    {
        static MapGraticule()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapGraticule), new FrameworkPropertyMetadata(false));

            MapOverlay.StrokeThicknessProperty.OverrideMetadata(
                typeof(MapGraticule), new FrameworkPropertyMetadata(0.5));
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
                var start = ParentMap.MapTransform.Transform(new Point(bounds.X, bounds.Y));
                var end = ParentMap.MapTransform.Transform(new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height));
                var minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, ParentMap.ZoomLevel) * TileSource.TileSize);
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
                        ParentMap.LocationToViewportPoint(new Location(lat, start.Longitude)),
                        ParentMap.LocationToViewportPoint(new Location(lat, end.Longitude)));
                }

                for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                {
                    drawingContext.DrawLine(Pen,
                        ParentMap.LocationToViewportPoint(new Location(start.Latitude, lon)),
                        ParentMap.LocationToViewportPoint(new Location(end.Latitude, lon)));
                }

                if (Foreground != null && Foreground != Brushes.Transparent)
                {
                    var format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

                    for (var lat = labelsStart.Latitude; lat <= end.Latitude; lat += spacing)
                    {
                        for (var lon = labelsStart.Longitude; lon <= end.Longitude; lon += spacing)
                        {
                            var t = StrokeThickness / 2d;
                            var p = ParentMap.LocationToViewportPoint(new Location(lat, lon));
                            var latPos = new Point(p.X + t + 2d, p.Y - t - FontSize / 4d);
                            var lonPos = new Point(p.X + t + 2d, p.Y + t + FontSize);
                            var latLabel = CoordinateString(lat, format, "NS");
                            var lonLabel = CoordinateString(Location.NormalizeLongitude(lon), format, "EW");

                            drawingContext.PushTransform(new RotateTransform(ParentMap.Heading, p.X, p.Y));
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
