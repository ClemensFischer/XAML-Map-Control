// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
#if WINDOWS_RUNTIME
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;

#endif

namespace MapControl
{
    public partial class MapPolyline
    {
        public MapPolyline()
        {
            Data = new PathGeometry();
        }

        protected override void UpdateData()
        {
            var geometry = (PathGeometry)Data;
            var locations = Locations;
            Location first;

            if (ParentMap != null && locations != null && (first = locations.FirstOrDefault()) != null)
            {
                var figure = new PathFigure
                {
                    StartPoint = ParentMap.MapTransform.Transform(first),
                    IsClosed = IsClosed,
                    IsFilled = IsClosed
                };

                var segment = new PolyLineSegment();

                foreach (var location in locations.Skip(1))
                {
                    segment.Points.Add(ParentMap.MapTransform.Transform(location));
                }

                if (segment.Points.Count > 0)
                {
                    figure.Segments.Add(segment);
                }

                geometry.Figures.Clear();
                geometry.Figures.Add(figure);
                geometry.Transform = ParentMap.ViewportTransform;
            }
            else
            {
                geometry.Figures.Clear();
                geometry.ClearValue(Geometry.TransformProperty);
            }
        }
    }
}
