// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
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
            typeof(MapScale), new FrameworkPropertyMetadata(new Thickness(2d)));

        private double length;
        private Size size;

        static MapScale()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapScale), new FrameworkPropertyMetadata(false));

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
            if (ParentMap != null && ParentMap.CenterScale > 0d)
            {
                length = MinWidth / ParentMap.CenterScale;
                var magnitude = Math.Pow(10d, Math.Floor(Math.Log10(length)));

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

                size.Width = length * ParentMap.CenterScale + StrokeThickness + Padding.Left + Padding.Right;
                size.Height = FontSize + 2d * StrokeThickness + Padding.Top + Padding.Bottom;
            }
            else
            {
                size.Width = size.Height = 0d;
            }

            return size;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var x1 = Padding.Left + StrokeThickness / 2d;
                var x2 = size.Width - Padding.Right - StrokeThickness / 2d;
                var y1 = size.Height / 2d;
                var y2 = size.Height - Padding.Bottom - StrokeThickness / 2d;
                var text = length >= 1000d ? string.Format("{0:0} km", length / 1000d) : string.Format("{0:0} m", length);

                drawingContext.DrawRectangle(Background ?? ParentMap.Background, null, new Rect(size));
                drawingContext.DrawLine(Pen, new Point(x1, y1), new Point(x1, y2));
                drawingContext.DrawLine(Pen, new Point(x2, y1), new Point(x2, y2));
                drawingContext.DrawLine(Pen, new Point(x1, y2), new Point(x2, y2));
                drawingContext.DrawGlyphRun(Foreground,
                    GlyphRunText.Create(text, Typeface, FontSize),
                    new Point(size.Width / 2d, y1 - StrokeThickness - 1d),
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        protected override void OnViewportChanged()
        {
            InvalidateMeasure();
        }
    }
}
