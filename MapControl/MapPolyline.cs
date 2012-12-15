// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            "Locations", typeof(IEnumerable<Location>), typeof(MapPolyline),
            new PropertyMetadata(null, LocationsPropertyChanged));

        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            "IsClosed", typeof(bool), typeof(MapPolyline),
            new PropertyMetadata(false, (o, e) => ((MapPolyline)o).UpdateGeometry()));

        protected readonly PathGeometry Geometry = new PathGeometry();

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
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

        void IMapElement.ParentMapChanged(MapBase oldParentMap, MapBase newParentMap)
        {
            UpdateGeometry();
        }

        private void LocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateGeometry();
        }

        private static void LocationsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var mapPolyline = (MapPolyline)obj;
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            var newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= mapPolyline.LocationCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += mapPolyline.LocationCollectionChanged;
            }

            mapPolyline.UpdateGeometry();
        }
    }
}
