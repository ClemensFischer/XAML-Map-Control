// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class CenteredBoundingBox : BoundingBox
    {
        public CenteredBoundingBox(Location center, double width, double height)
        {
            Center = center;
            Width = Math.Max(width, 0d);
            Height = Math.Max(height, 0d);
        }

        public override double Width { get; protected set; }
        public override double Height { get; protected set; }
        public override Location Center { get; protected set; }
    }
}
