// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// Rotated rectangle used to arrange and rotate an element with a BoundingBox.
    /// </summary>
    public class ViewRect
    {
        public ViewRect(double x, double y, double width, double height, double rotation)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rotation = rotation;
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
        public double Rotation { get; }
    }
}
