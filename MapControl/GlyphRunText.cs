// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
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
        public static GlyphRun Create(string text, Typeface typeface, double emSize, Point baselineOrigin)
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

            return new GlyphRun(glyphTypeface, 0, false, emSize, glyphIndices, baselineOrigin, advanceWidths,
                                null, null, null, null, null, null);
        }

        public static GlyphRun Create(string text, Typeface typeface, double emSize, Vector centerOffset)
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

            var glyphRun = new GlyphRun(glyphTypeface, 0, false, emSize, glyphIndices, new Point(), advanceWidths,
                                        null, null, null, null, null, null);
            var bbox = glyphRun.ComputeInkBoundingBox();
            var baselineOrigin = new Point(centerOffset.X - bbox.X - bbox.Width / 2d, centerOffset.Y - bbox.Y - bbox.Height / 2d);

            return new GlyphRun(glyphTypeface, 0, false, emSize, glyphIndices, baselineOrigin, advanceWidths,
                                null, null, null, null, null, null);
        }
    }
}
