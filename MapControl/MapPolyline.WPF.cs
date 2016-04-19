// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapPolyline
    {
        public static readonly DependencyProperty FillRuleProperty = StreamGeometry.FillRuleProperty.AddOwner(
            typeof(MapPolyline),
            new FrameworkPropertyMetadata((o, e) => ((StreamGeometry)((MapPolyline)o).Data).FillRule = (FillRule)e.NewValue));

        public MapPolyline()
        {
            Data = new StreamGeometry { Transform = ViewportTransform };
        }

        protected override void UpdateData()
        {
            var geometry = (StreamGeometry)Data;

            if (ParentMap != null && Locations != null && Locations.Any())
            {
                using (var context = geometry.Open())
                {
                    var points = Locations.Select(l => ParentMap.MapTransform.Transform(l));

                    context.BeginFigure(points.First(), IsClosed, IsClosed);
                    context.PolyLineTo(points.Skip(1).ToList(), true, false);
                }
            }
            else
            {
                geometry.Clear();
            }
        }
    }
}
