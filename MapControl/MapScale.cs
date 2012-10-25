// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Draws a map scale overlay.
    /// </summary>
    public class MapScale : MapOverlay
    {
        public static readonly DependencyProperty PaddingProperty = Control.PaddingProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata(new Thickness(2d)));

        private double length;
        private Size size;

        static MapScale()
        {
            FrameworkElement.MinWidthProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(100d));

            FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(HorizontalAlignment.Right));

            FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(VerticalAlignment.Bottom));

            MapOverlay.StrokeStartLineCapProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(PenLineCap.Round));

            MapOverlay.StrokeEndLineCapProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(PenLineCap.Round));
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double scale = ParentMap.CenterScale; // px/m
            length = MinWidth / scale;
            double magnitude = Math.Pow(10d, Math.Floor(Math.Log10(length)));

            if (length / magnitude < 2d)
            {
                length = 2d * magnitude;
            }
            else if (length / magnitude < 5d)
            {
                length = 5d * magnitude;
            }
            else
            {
                length = 10d * magnitude;
            }

            size.Width = length * scale + StrokeThickness + Padding.Left + Padding.Right;
            size.Height = FontSize + 2d * StrokeThickness + Padding.Top + Padding.Bottom;
            return size;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double x1 = Padding.Left + StrokeThickness / 2d;
            double x2 = size.Width - Padding.Right - StrokeThickness / 2d;
            double y1 = size.Height / 2d;
            double y2 = size.Height - Padding.Bottom - StrokeThickness / 2d;
            string text = length >= 1000d ? string.Format("{0:0} km", length / 1000d) : string.Format("{0:0} m", length);

            drawingContext.DrawRectangle(Background ?? ParentMap.Background, null, new Rect(size));
            drawingContext.DrawLine(Pen, new Point(x1, y1), new Point(x1, y2));
            drawingContext.DrawLine(Pen, new Point(x2, y1), new Point(x2, y2));
            drawingContext.DrawLine(Pen, new Point(x1, y2), new Point(x2, y2));
            drawingContext.DrawGlyphRun(Foreground,
                GlyphRunText.Create(text, Typeface, FontSize, new Vector(size.Width / 2d, y1 - StrokeThickness - 1d)));
        }

        protected override void OnViewportChanged()
        {
            InvalidateMeasure();
        }
    }
}
