// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapPolyline
    {
        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            "FillRule", typeof(FillRule), typeof(MapPolyline),
            new PropertyMetadata(FillRule.EvenOdd, (o, e) => ((PathGeometry)((MapPolyline)o).Data).FillRule = (FillRule)e.NewValue));

        public MapPolyline()
        {
            Data = new PathGeometry { Transform = ViewportTransform };
        }

        protected override void UpdateData()
        {
            var geometry = (PathGeometry)Data;
            geometry.Figures.Clear();

            if (ParentMap != null && Locations != null && Locations.Any())
            {
                var points = Locations.Select(l => ParentMap.MapTransform.Transform(l));

                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = IsClosed,
                    IsFilled = IsClosed
                };

                var segment = new PolyLineSegment();

                foreach (var point in points.Skip(1))
                {
                    segment.Points.Add(point);
                }

                figure.Segments.Add(segment);
                geometry.Figures.Add(figure);
            }
        }
    }
}
