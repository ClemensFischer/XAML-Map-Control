// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    /// <summary>
    /// A polyline defined by a collection of Locations.
    /// </summary>
    public class MapPolyline : MapShape
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            nameof(Locations), typeof(IEnumerable<Location>), typeof(MapPolyline),
            new PropertyMetadata(null, (o, e) => ((MapPolyline)o).LocationsPropertyChanged(e)));

        /// <summary>
        /// Gets or sets the Locations that define the polyline points.
        /// </summary>
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected override void UpdateData()
        {
            var geometry = (PathGeometry)Data;
            geometry.Figures.Clear();

            if (ParentMap != null && Locations != null && Locations.Any())
            {
                PathFigure figure = null;
                PolyLineSegment segment = null;
                var size = ParentMap.RenderSize;
                var offset = GetLongitudeOffset();
                var locations = Locations;

                if (offset != 0d)
                {
                    locations = locations.Select(loc => new Location(loc.Latitude, loc.Longitude + offset));
                }

                var points = locations.Select(loc => ParentMap.MapProjection.LocationToViewportPoint(loc));
                var p1 = points.First();

                foreach (var p2 in points.Skip(1))
                {
                    if ((p1.X <= 0 && p2.X <= 0) || (p1.X >= size.Width && p2.X >= size.Width) ||
                        (p1.Y <= 0 && p2.Y <= 0) || (p1.Y >= size.Height && p2.Y >= size.Height))
                    {
                        // line (p1,p2) is out of visible bounds, end figure
                        figure = null;
                    }
                    else
                    {
                        if (figure == null)
                        {
                            figure = new PathFigure { StartPoint = p1, IsClosed = false, IsFilled = false };
                            segment = new PolyLineSegment();
                            figure.Segments.Add(segment);
                            geometry.Figures.Add(figure);
                        }

                        segment.Points.Add(p2);
                    }

                    p1 = p2;
                }
            }
        }

        private void LocationsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            var newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= LocationCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += LocationCollectionChanged;
            }

            UpdateData();
        }

        private void LocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }
    }
}
