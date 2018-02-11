// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
            IsHitTestVisibleProperty.OverrideMetadata(typeof(MapGraticule), new FrameworkPropertyMetadata(false));
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
                var lineDistance = GetLineDistance();
                var labelFormat = GetLabelFormat(lineDistance);

                if (projection.IsAzimuthal)
                {

                }
                else
                {
                    DrawCylindricalGraticule(drawingContext, projection, lineDistance, labelFormat);
                }
            }
        }

        private void DrawCylindricalGraticule(DrawingContext drawingContext, MapProjection projection, double lineDistance, string labelFormat)
        {
            var boundingBox = projection.ViewportRectToBoundingBox(new Rect(ParentMap.RenderSize));

            if (boundingBox.HasValidBounds)
            {
                var latLabelStart = Math.Ceiling(boundingBox.South / lineDistance) * lineDistance;
                var lonLabelStart = Math.Ceiling(boundingBox.West / lineDistance) * lineDistance;
                var latLabels = new List<Label>((int)((boundingBox.North - latLabelStart) / lineDistance) + 1);
                var lonLabels = new List<Label>((int)((boundingBox.East - lonLabelStart) / lineDistance) + 1);
                var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                var pen = new Pen
                {
                    Brush = Stroke,
                    Thickness = StrokeThickness,
                    DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset),
                    DashCap = StrokeDashCap
                };

                for (var lat = latLabelStart; lat <= boundingBox.North; lat += lineDistance)
                {
                    latLabels.Add(new Label(lat, new FormattedText(
                        GetLabelText(lat, labelFormat, "NS"),
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground)));

                    drawingContext.DrawLine(pen,
                        projection.LocationToViewportPoint(new Location(lat, boundingBox.West)),
                        projection.LocationToViewportPoint(new Location(lat, boundingBox.East)));
                }

                for (var lon = lonLabelStart; lon <= boundingBox.East; lon += lineDistance)
                {
                    lonLabels.Add(new Label(lon, new FormattedText(
                        GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW"),
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground)));

                    drawingContext.DrawLine(pen,
                        projection.LocationToViewportPoint(new Location(boundingBox.South, lon)),
                        projection.LocationToViewportPoint(new Location(boundingBox.North, lon)));
                }

                foreach (var latLabel in latLabels)
                {
                    foreach (var lonLabel in lonLabels)
                    {
                        var position = projection.LocationToViewportPoint(new Location(latLabel.Position, lonLabel.Position));

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
