// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
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

                if (projection.IsNormalCylindrical)
                {
                    DrawCylindricalGraticule(drawingContext, lineDistance, labelFormat);
                }
                else
                {
                }
            }
        }

        private void DrawCylindricalGraticule(DrawingContext drawingContext, double lineDistance, string labelFormat)
        {
            var boundingBox = ParentMap.ViewRectToBoundingBox(new Rect(ParentMap.RenderSize));
            var latLabelStart = Math.Ceiling(boundingBox.South / lineDistance) * lineDistance;
            var lonLabelStart = Math.Ceiling(boundingBox.West / lineDistance) * lineDistance;
            var latLabels = new List<Label>((int)((boundingBox.North - latLabelStart) / lineDistance) + 1);
            var lonLabels = new List<Label>((int)((boundingBox.East - lonLabelStart) / lineDistance) + 1);
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var pen = CreatePen();

            for (var lat = latLabelStart; lat <= boundingBox.North; lat += lineDistance)
            {
                latLabels.Add(new Label(lat, new FormattedText(
                    GetLabelText(lat, labelFormat, "NS"),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip)));

                drawingContext.DrawLine(pen,
                    ParentMap.LocationToView(new Location(lat, boundingBox.West)),
                    ParentMap.LocationToView(new Location(lat, boundingBox.East)));
            }

            for (var lon = lonLabelStart; lon <= boundingBox.East; lon += lineDistance)
            {
                lonLabels.Add(new Label(lon, new FormattedText(
                    GetLabelText(Location.NormalizeLongitude(lon), labelFormat, "EW"),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground, pixelsPerDip)));

                drawingContext.DrawLine(pen,
                    ParentMap.LocationToView(new Location(boundingBox.South, lon)),
                    ParentMap.LocationToView(new Location(boundingBox.North, lon)));
            }

            foreach (var latLabel in latLabels)
            {
                foreach (var lonLabel in lonLabels)
                {
                    var position = ParentMap.LocationToView(new Location(latLabel.Position, lonLabel.Position));

                    drawingContext.PushTransform(new RotateTransform(ParentMap.ViewTransform.Rotation, position.X, position.Y));
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
