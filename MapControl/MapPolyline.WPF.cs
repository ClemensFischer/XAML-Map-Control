// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Shape
    {
        protected readonly StreamGeometry Geometry = new StreamGeometry();

        protected override Geometry DefiningGeometry
        {
            get { return Geometry; }
        }

        private void UpdateGeometry()
        {
            var parentMap = MapPanel.GetParentMap(this);
            var locations = Locations;
            Location first;

            if (parentMap != null && locations != null && (first = locations.FirstOrDefault()) != null)
            {
                using (var context = Geometry.Open())
                {
                    var startPoint = parentMap.MapTransform.Transform(first);
                    var points = locations.Skip(1).Select(l => parentMap.MapTransform.Transform(l)).ToList();

                    context.BeginFigure(startPoint, IsClosed, IsClosed);
                    context.PolyLineTo(points, true, false);
                }

                Geometry.Transform = parentMap.ViewportTransform;
            }
            else
            {
                Geometry.Clear();
                Geometry.Transform = null;
            }
        }
    }
}
