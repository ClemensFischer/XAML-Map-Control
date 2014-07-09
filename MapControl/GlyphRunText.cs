// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Contains helper methods for creating GlyphRun objects.
    /// </summary>
    public static class GlyphRunText
    {
        public static GlyphRun Create(string text, Typeface typeface, double emSize, Point baselineOrigin = new Point())
        {
            GlyphTypeface glyphTypeface;

            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                throw new ArgumentException(string.Format("{0}: no GlyphTypeface found", typeface.FontFamily));
            }

            var glyphIndices = new ushort[text.Length];
            var advanceWidths = new double[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[i]];
                glyphIndices[i] = glyphIndex;
                advanceWidths[i] = glyphTypeface.AdvanceWidths[glyphIndex] * emSize;
            }

            return new GlyphRun(glyphTypeface, 0, false, emSize, glyphIndices, baselineOrigin, advanceWidths, null, null, null, null, null, null);
        }

        public static void DrawGlyphRun(this DrawingContext drawingContext, Brush foreground, GlyphRun glyphRun,
            Point position, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            var boundingBox = glyphRun.ComputeInkBoundingBox();

            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    position.X -= boundingBox.Width / 2d;
                    break;
                case HorizontalAlignment.Right:
                    position.X -= boundingBox.Width;
                    break;
                default:
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Center:
                    position.Y -= boundingBox.Height / 2d;
                    break;
                case VerticalAlignment.Bottom:
                    position.Y -= boundingBox.Height;
                    break;
                default:
                    break;
            }

            drawingContext.PushTransform(new TranslateTransform(position.X - boundingBox.X, position.Y - boundingBox.Y));
            drawingContext.DrawGlyphRun(foreground, glyphRun);
            drawingContext.Pop();
        }
    }
}
