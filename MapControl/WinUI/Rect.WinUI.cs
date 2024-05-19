// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Rect for double floating point precision.
    /// </summary>
    public readonly struct Rect
    {
        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }


        public Rect(Point p1, Point p2)
            : this(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y)
        {
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public bool Contains(Point p) => p.X >= X && p.X <= X + Width && p.Y >= Y && p.Y <= Y + Height;

        public static implicit operator Windows.Foundation.Rect(Rect r)
        {
            return new Windows.Foundation.Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static implicit operator Rect(Windows.Foundation.Rect r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }
    }
}
