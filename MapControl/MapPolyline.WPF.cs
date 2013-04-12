// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapPolyline
    {
        public MapPolyline()
            : base(new StreamGeometry())
        {
        }

        protected override void UpdateGeometry()
        {
            var geometry = (StreamGeometry)Geometry;
            var locations = Locations;
            Location first;

            if (ParentMap != null && locations != null && (first = locations.FirstOrDefault()) != null)
            {
                using (var context = geometry.Open())
                {
                    var startPoint = ParentMap.MapTransform.Transform(first);
                    var points = locations.Skip(1).Select(l => ParentMap.MapTransform.Transform(l)).ToList();

                    context.BeginFigure(startPoint, IsClosed, IsClosed);
                    context.PolyLineTo(points, true, false);
                }

                geometry.Transform = ParentMap.ViewportTransform;
            }
            else
            {
                geometry.Clear();
                geometry.ClearValue(Geometry.TransformProperty);
            }
        }
    }
}
