// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class CenteredBoundingBox : BoundingBox
    {
        private readonly Location center;
        private readonly double width;
        private readonly double height;

        public CenteredBoundingBox(Location center, double width, double height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
        }

        public Location Center
        {
            get { return center; }
        }

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
            return new CenteredBoundingBox(center, width, height);
        }
    }
}
