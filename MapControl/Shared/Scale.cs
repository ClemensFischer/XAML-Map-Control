// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public readonly struct Scale
    {
        public Scale(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static Scale operator *(double f, Scale v)
        {
            return new Scale(f * v.X, f * v.Y);
        }
    }
}
