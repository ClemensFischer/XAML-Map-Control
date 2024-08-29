// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
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

        public override Location Center { get; set; }
        public override double Width { get; }
        public override double Height { get; }
    }
}
