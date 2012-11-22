// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
#if WINRT
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
            new PropertyMetadata(null, LocationsPropertyChanged));

        protected PathGeometry Geometry = new PathGeometry();

        public MapPolyline()
        {
            MapPanel.AddParentMapHandlers(this);
            Initialize();
        }

        partial void Initialize();

        public LocationCollection Locations
        {
            get { return (LocationCollection)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected virtual bool IsClosed
        {
            get { return false; }
        }

        protected virtual void UpdateGeometry(MapBase parentMap, LocationCollection locations)
        {
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
            UpdateGeometry(newParentMap, Locations);
        }

        private static void LocationsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var polyline = (MapPolyline)obj;
            polyline.UpdateGeometry(MapPanel.GetParentMap(polyline), (LocationCollection)e.NewValue);
        }
    }
}
