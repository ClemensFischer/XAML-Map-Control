// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Map rectangle with double floating point precision, in contrast to Windows.Foundation.Rect.
    /// Used by MapProjection when converting geographic bounding boxes to/from projected map coordinates.
    /// </summary>
    public class MapRect
    {
        public MapRect(double x1, double y1, double x2, double y2)
        {
            XMin = Math.Min(x1, x2);
            YMin = Math.Min(y1, y2);
            XMax = Math.Max(x1, x2);
            YMax = Math.Max(y1, y2);
        }

        public MapRect(Point point1, Point point2)
            : this(point1.X, point1.Y, point2.X, point2.Y)
        {
        }

        public double XMin { get; }
        public double YMin { get; }
        public double XMax { get; }
        public double YMax { get; }

        public double Width => XMax - XMin;
        public double Height => YMax - YMin;

        public Point Center => new Point((XMin + XMax) / 2d, (YMin + YMax) / 2d);

        public bool Contains(Point p) => p.X >= XMin && p.X <= XMax && p.Y >= YMin && p.Y <= YMax;
    }
}
