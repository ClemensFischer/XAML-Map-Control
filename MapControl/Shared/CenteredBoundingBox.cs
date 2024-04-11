// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class CenteredBoundingBox : BoundingBox
    {
        private readonly Location center;
        private readonly double width;
        private readonly double height;

        public CenteredBoundingBox(Location c, double w, double h)
        {
            center = c;
            width = Math.Max(w, 0d);
            height = Math.Max(h, 0d);
        }

        public override Location Center => center;
        public override double Width => width;
        public override double Height => height;
    }
}
