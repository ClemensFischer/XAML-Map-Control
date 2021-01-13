// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class CenteredBoundingBox : BoundingBox
    {
        private readonly double width;
        private readonly double height;

        public CenteredBoundingBox(Location center, double width, double height)
        {
            Center = center;
            this.width = Math.Max(width, 0d);
            this.height = Math.Max(height, 0d);
        }

        public Location Center { get; private set; }

        public override double Width
        {
            get { return width; }
        }

        public override double Height
        {
            get { return height; }
        }

        public override BoundingBox Clone()
        {
            return new CenteredBoundingBox(Center, Width, Height);
        }
    }
}
