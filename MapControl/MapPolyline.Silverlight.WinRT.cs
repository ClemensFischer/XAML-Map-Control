// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    public partial class MapPolyline : Path
    {
        protected readonly PathGeometry Geometry = new PathGeometry();

        public MapPolyline()
        {
            Data = Geometry;
            MapPanel.AddParentMapHandlers(this);
        }

        private void UpdateGeometry()
        {
            var parentMap = MapPanel.GetParentMap(this);
            var locations = Locations;
            Location first;

            Geometry.Figures.Clear();

            if (parentMap != null && locations != null && (first = locations.FirstOrDefault()) != null)
            {
                var figure = new PathFigure
                {
                    StartPoint = parentMap.MapTransform.Transform(first),
                    IsClosed = IsClosed,
                    IsFilled = IsClosed
                };

                var segment = new PolyLineSegment();

                foreach (var location in locations.Skip(1))
                {
                    segment.Points.Add(parentMap.MapTransform.Transform(location));
                }

                if (segment.Points.Count > 0)
                {
                    figure.Segments.Add(segment);
                }

                Geometry.Figures.Add(figure);
                Geometry.Transform = parentMap.ViewportTransform;
            }
            else
            {
                Geometry.Transform = null;
            }
        }
    }
}
