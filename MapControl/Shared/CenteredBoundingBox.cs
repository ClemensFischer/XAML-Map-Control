using System;

namespace MapControl
{
    public class CenteredBoundingBox(Location center, double width, double height) : BoundingBox
    {
        public override Location Center { get; } = center;
        public override double Width { get; } = Math.Max(width, 0d);
        public override double Height { get; } = Math.Max(height, 0d);
    }
}
