// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Path
    {
        public MapPolyline()
        {
            Data = Geometry;
            MapPanel.AddParentMapHandlers(this);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // MeasureOverride in Silverlight occasionally tries to create
            // negative width or height from a transformed geometry in Data.
            return new Size(Geometry.Bounds.Width, Geometry.Bounds.Height);
        }
    }
}
