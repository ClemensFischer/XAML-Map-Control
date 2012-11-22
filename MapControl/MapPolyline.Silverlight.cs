// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Path
    {
        partial void Initialize()
        {
            Data = Geometry;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(
                Math.Max(Geometry.Bounds.Width, Geometry.Bounds.Right),
                Math.Max(Geometry.Bounds.Height, Geometry.Bounds.Bottom));
        }
    }
}
