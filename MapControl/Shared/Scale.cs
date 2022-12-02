// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public struct Scale
    {
        public Scale(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public static Scale operator *(double f, Scale v)
        {
            return new Scale(f * v.X, f * v.Y);
        }
    }
}
