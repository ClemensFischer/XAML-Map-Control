// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Rotated rectangle used to arrange and rotate an element with a BoundingBox.
    /// </summary>
    public readonly struct ViewRect
    {
        public ViewRect(double x, double y, double width, double height, double rotation)
        {
            Rect = new Rect(x, y, width, height);
            Rotation = rotation;
        }

        public Rect Rect { get; }
        public double Rotation { get; }
    }
}
