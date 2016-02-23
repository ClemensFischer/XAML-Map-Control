// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapGraticule
    {
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
            IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapGraticule), new FrameworkPropertyMetadata(false));

            StrokeThicknessProperty.OverrideMetadata(
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
                var lineDistance = GetLineDistance();
                var labelFormat = GetLabelFormat(lineDistance);
                var latLabelStart = Math.Ceiling(start.Latitude / lineDistance) * lineDistance;
                var lonLabelStart = Math.Ceiling(start.Longitude / lineDistance) * lineDistance;
                var latLabels = new List<Label>((int)((end.Latitude - latLabelStart) / lineDistance) + 1);
                var lonLabels = new List<Label>((int)((end.Longitude - lonLabelStart) / lineDistance) + 1);

                for (var lat = latLabelStart; lat <= end.Latitude; lat += lineDistance)
                {
                    latLabels.Add(new Label(lat, new FormattedText(
                        GetLabelText(lat, labelFormat, "NS"),
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Foreground)));

                    drawingContext.DrawLine(Pen,
                        ParentMap.LocationToViewportPoint(new Location(lat, start.Longitude)),
                        ParentMap.LocationToViewportPoint(new Location(lat, end.Longitude)));
                }

                for (var lon = lonLabelStart; lon <= end.Longitude; lon += lineDistance)
                {
                    lonLabels.Add(new Label(lon, new FormattedText(
                        GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW"),
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Foreground)));

                    drawingContext.DrawLine(Pen,
                        ParentMap.LocationToViewportPoint(new Location(start.Latitude, lon)),
                        ParentMap.LocationToViewportPoint(new Location(end.Latitude, lon)));
                }

                foreach (var latLabel in latLabels)
                {
                    foreach (var lonLabel in lonLabels)
                    {
                        var position = ParentMap.LocationToViewportPoint(new Location(latLabel.Position, lonLabel.Position));

                        drawingContext.PushTransform(new RotateTransform(ParentMap.Heading, position.X, position.Y));
                        drawingContext.DrawText(latLabel.Text,
                            new Point(position.X + StrokeThickness / 2d + 2d, position.Y - StrokeThickness / 2d - latLabel.Text.Height));
                        drawingContext.DrawText(lonLabel.Text,
                            new Point(position.X + StrokeThickness / 2d + 2d, position.Y + StrokeThickness / 2d));
                        drawingContext.Pop();
                    }
                }
            }
        }
    }
}
