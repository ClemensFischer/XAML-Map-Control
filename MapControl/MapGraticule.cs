// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Draws a graticule overlay.
    /// </summary>
    public class MapGraticule : MapOverlay
    {
        public static readonly DependencyProperty MinLineSpacingProperty = DependencyProperty.Register(
            "MinLineSpacing", typeof(double), typeof(MapGraticule), new FrameworkPropertyMetadata(100d));

        /// <summary>
        /// Graticule line spacings in degrees.
        /// </summary>
        public static double[] LineSpacings =
            new double[] { 1d / 60d, 1d / 30d, 1d / 12d, 1d / 6d, 1d / 4d, 1d / 3d, 1d / 2d, 1d, 2d, 5d, 10d, 15d, 20d, 30d, 45d };

        /// <summary>
        /// Minimum spacing in pixels between adjacent graticule lines.
        /// </summary>
        public double MinLineSpacing
        {
            get { return (double)GetValue(MinLineSpacingProperty); }
            set { SetValue(MinLineSpacingProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            MapBase parentMap = ParentMap;
            Rect bounds = parentMap.ViewportTransform.Inverse.TransformBounds(new Rect(parentMap.RenderSize));
            Location loc1 = parentMap.MapTransform.TransformBack(bounds.TopLeft);
            Location loc2 = parentMap.MapTransform.TransformBack(bounds.BottomRight);
            double minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, parentMap.ZoomLevel) * 256d);
            double spacing = LineSpacings[LineSpacings.Length - 1];

            if (spacing >= minSpacing)
            {
                spacing = LineSpacings.FirstOrDefault(s => s >= minSpacing);
            }

            double latitudeStart = Math.Ceiling(loc1.Latitude / spacing) * spacing;
            double longitudeStart = Math.Ceiling(loc1.Longitude / spacing) * spacing;

            for (double lat = latitudeStart; lat <= loc2.Latitude; lat += spacing)
            {
                drawingContext.DrawLine(Pen,
                    parentMap.LocationToViewportPoint(new Location(lat, loc1.Longitude)),
                    parentMap.LocationToViewportPoint(new Location(lat, loc2.Longitude)));
            }

            for (double lon = longitudeStart; lon <= loc2.Longitude; lon += spacing)
            {
                drawingContext.DrawLine(Pen,
                    parentMap.LocationToViewportPoint(new Location(loc1.Latitude, lon)),
                    parentMap.LocationToViewportPoint(new Location(loc2.Latitude, lon)));
            }

            if (Foreground != null && Foreground != Brushes.Transparent)
            {
                string format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

                for (double lat = latitudeStart; lat <= loc2.Latitude; lat += spacing)
                {
                    for (double lon = longitudeStart; lon <= loc2.Longitude; lon += spacing)
                    {
                        double t = StrokeThickness / 2d;
                        Point p = parentMap.LocationToViewportPoint(new Location(lat, lon));
                        Point latPos = new Point(p.X + t + 2d, p.Y - t - FontSize / 4d);
                        Point lonPos = new Point(p.X + t + 2d, p.Y + t + FontSize);
                        string latString = CoordinateString(lat, format, "NS");
                        string lonString = CoordinateString(Location.NormalizeLongitude(lon), format, "EW");

                        drawingContext.PushTransform(new RotateTransform(parentMap.Heading, p.X, p.Y));
                        drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(latString, Typeface, FontSize, latPos));
                        drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(lonString, Typeface, FontSize, lonPos));
                        drawingContext.Pop();
                    }
                }
            }
        }

        private static string CoordinateString(double value, string format, string hemispheres)
        {
            char hemisphere = hemispheres[0];

            if (value < -1e-8) // ~1mm
            {
                value = -value;
                hemisphere = hemispheres[1];
            }

            int minutes = (int)(value * 60d + 0.5);

            return string.Format(format, hemisphere, minutes / 60, (double)(minutes % 60));
        }
    }
}
