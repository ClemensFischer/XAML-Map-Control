// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public abstract partial class MapShape : Shape, IWeakEventListener
    {
        public Geometry Data { get; }

        protected override Geometry DefiningGeometry
        {
            get { return Data; }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            UpdateData();
            return true;
        }

        protected void DataCollectionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            INotifyCollectionChanged collection;

            if ((collection = e.OldValue as INotifyCollectionChanged) != null)
            {
                CollectionChangedEventManager.RemoveListener(collection, this);
            }

            if ((collection = e.NewValue as INotifyCollectionChanged) != null)
            {
                CollectionChangedEventManager.AddListener(collection, this);
            }

            UpdateData();
        }

        protected void AddPolylineLocations(PathFigureCollection figures, IEnumerable<Location> locations, bool closed)
        {
            if (locations != null && locations.Count() >= 2)
            {
                var offset = GetLongitudeOffset();
                if (offset != 0d)
                {
                    locations = locations.Select(loc => new Location(loc.Latitude, loc.Longitude + offset));
                }

                var points = locations.Select(loc => LocationToViewportPoint(loc));
                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = closed,
                    IsFilled = closed
                };

                figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
                figures.Add(figure);
            }
        }
    }
}
