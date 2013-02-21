// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
#if NETFX_CORE
using Windows.Foundation;
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

        protected override Size MeasureOverride(Size constraint)
        {
            // The Silverlight Path.MeasureOverride occasionally tries to create a Size from
            // a negative width or height, apparently resulting from a transformed geometry
            // in Path.Data. It seems to be sufficient to always return a non-zero size.
            return new Size(1, 1);
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
