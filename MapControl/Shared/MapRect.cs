// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Map rectangle with double floating point precision, in contrast to Windows.Foundation.Rect.
    /// Used by MapProjection to convert geodetic bounding boxes to/from projected map coordinates.
    /// </summary>
    public class MapRect
    {
        public MapRect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public MapRect(Point p1, Point p2)
        {
            X = Math.Min(p1.X, p2.X);
            Y = Math.Min(p1.Y, p2.Y);
            Width = Math.Max(p1.X, p2.X) - X;
            Height = Math.Max(p1.Y, p2.Y) - Y;
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public bool Contains(Point p)
        {
            return p.X >= X && p.X <= X + Width
                && p.Y >= Y && p.Y <= Y + Height;
        }
    }
}
