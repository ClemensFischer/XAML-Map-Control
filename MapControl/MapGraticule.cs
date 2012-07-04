// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    /// <summary>
    /// Draws a graticule overlay. The minimum spacing in pixels between adjacent
    /// graticule lines is specified by the MinLineSpacing property.
    /// </summary>
    public class MapGraticule : MapElement
    {
        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).UpdateBrush()));

        public static readonly DependencyProperty FontSizeProperty = Control.FontSizeProperty.AddOwner(
            typeof(MapGraticule));

        public static readonly DependencyProperty FontFamilyProperty = Control.FontFamilyProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).typeface = null));

        public static readonly DependencyProperty FontStyleProperty = Control.FontStyleProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).typeface = null));

        public static readonly DependencyProperty FontWeightProperty = Control.FontWeightProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).typeface = null));

        public static readonly DependencyProperty FontStretchProperty = Control.FontStretchProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).typeface = null));

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata((o, e) => ((MapGraticule)o).UpdateBrush()));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapGraticule), new FrameworkPropertyMetadata(0.5, (o, e) => ((MapGraticule)o).pen.Thickness = (double)e.NewValue));

        public static readonly DependencyProperty MinLineSpacingProperty = DependencyProperty.Register(
            "MinLineSpacing", typeof(double), typeof(MapGraticule), new FrameworkPropertyMetadata(100d));

        public static double[] Spacings =
            new double[] { 1d / 60d, 1d / 30d, 1d / 12d, 1d / 6d, 1d / 4d, 1d / 3d, 1d / 2d, 1d, 2d, 5d, 10d, 15d, 20d, 30d, 45d };

        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly Pen pen;
        private Typeface typeface;

        public MapGraticule()
        {
            pen = new Pen(null, StrokeThickness);
            IsHitTestVisible = false;
            AddVisualChild(visual);
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>
        /// Minimum spacing in pixels between adjacent graticule lines.
        /// </summary>
        public double MinLineSpacing
        {
            get { return (double)GetValue(MinLineSpacingProperty); }
            set { SetValue(MinLineSpacingProperty, value); }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visual;
        }

        protected override void OnViewTransformChanged(Map parentMap)
        {
            Rect bounds = parentMap.ViewportTransform.Inverse.TransformBounds(new Rect(parentMap.RenderSize));
            Location loc1 = parentMap.MapTransform.TransformBack(bounds.TopLeft);
            Location loc2 = parentMap.MapTransform.TransformBack(bounds.BottomRight);
            double minSpacing = MinLineSpacing * 360d / (Math.Pow(2d, parentMap.ZoomLevel) * 256d);
            double spacing = Spacings[Spacings.Length - 1];

            if (spacing >= minSpacing)
            {
                spacing = Spacings.FirstOrDefault(s => s >= minSpacing);
            }

            double latitudeStart = Math.Ceiling(loc1.Latitude / spacing) * spacing;
            double longitudeStart = Math.Ceiling(loc1.Longitude / spacing) * spacing;

            if (pen.Brush == null)
            {
                pen.Brush = Stroke != null ? Stroke : Foreground;
            }

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                for (double lat = latitudeStart; lat <= loc2.Latitude; lat += spacing)
                {
                    drawingContext.DrawLine(pen,
                        parentMap.LocationToViewportPoint(new Location(lat, loc1.Longitude)),
                        parentMap.LocationToViewportPoint(new Location(lat, loc2.Longitude)));
                }

                for (double lon = longitudeStart; lon <= loc2.Longitude; lon += spacing)
                {
                    drawingContext.DrawLine(pen,
                        parentMap.LocationToViewportPoint(new Location(loc1.Latitude, lon)),
                        parentMap.LocationToViewportPoint(new Location(loc2.Latitude, lon)));
                }

                if (Foreground != null && Foreground != Brushes.Transparent)
                {
                    string format = spacing < 1d ? "{0} {1}°{2:00}'" : "{0} {1}°";

                    if (typeface == null)
                    {
                        typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                    }

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
                            drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(latString, typeface, FontSize, latPos));
                            drawingContext.DrawGlyphRun(Foreground, GlyphRunText.Create(lonString, typeface, FontSize, lonPos));
                            drawingContext.Pop();
                        }
                    }
                }
            }
        }

        private void UpdateBrush()
        {
            pen.Brush = null;
            OnViewTransformChanged(ParentMap);
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
