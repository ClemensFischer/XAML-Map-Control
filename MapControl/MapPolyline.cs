// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
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
    public partial class MapPolyline : IMapElement
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(LocationCollection), typeof(MapPolyline),
            new PropertyMetadata(null, (o, e) => ((MapPolyline)o).UpdateGeometry()));

        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            "IsClosed", typeof(bool), typeof(MapPolyline),
            new PropertyMetadata(false, (o, e) => ((MapPolyline)o).UpdateGeometry()));

        protected PathGeometry Geometry = new PathGeometry();

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
        public LocationCollection Locations
        {
            get { return (LocationCollection)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the polyline is closed, i.e. is a polygon.
        /// </summary>
        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        protected virtual void UpdateGeometry()
        {
            var parentMap = MapPanel.GetParentMap(this);
            var locations = Locations;

            if (parentMap != null && locations != null && locations.Count > 0)
            {
                var figure = new PathFigure
                {
                    StartPoint = parentMap.MapTransform.Transform(locations[0]),
                    IsClosed = IsClosed,
                    IsFilled = IsClosed
                };

                if (locations.Count > 1)
                {
                    var segment = new PolyLineSegment();

                    foreach (Location location in locations.Skip(1))
                    {
                        segment.Points.Add(parentMap.MapTransform.Transform(location));
                    }

                    figure.Segments.Add(segment);
                }

                Geometry.Figures.Add(figure);
                Geometry.Transform = parentMap.ViewportTransform;
            }
        }

        void IMapElement.ParentMapChanged(MapBase oldParentMap, MapBase newParentMap)
        {
            UpdateGeometry();
        }
    }
}
