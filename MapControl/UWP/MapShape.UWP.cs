// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public abstract partial class MapShape : Path
    {
        protected void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected void DataCollectionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            INotifyCollectionChanged collection;

            if ((collection = e.OldValue as INotifyCollectionChanged) != null)
            {
                collection.CollectionChanged -= DataCollectionChanged;
            }

            if ((collection = e.NewValue as INotifyCollectionChanged) != null)
            {
                collection.CollectionChanged += DataCollectionChanged;
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

                var points = locations.Select(loc => LocationToViewportPoint(loc)).ToList();
                if (closed)
                {
                    points.Add(points[0]);
                }

                var viewport = new Rect(0, 0, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height);
                PathFigure figure = null;
                PolyLineSegment segment = null;

                for (int i = 1; i < points.Count; i++)
                {
                    var p1 = points[i - 1];
                    var p2 = points[i];
                    var inside = Intersections.GetIntersections(ref p1, ref p2, viewport);

                    if (inside)
                    {
                        if (figure == null)
                        {
                            figure = new PathFigure
                            {
                                StartPoint = p1,
                                IsClosed = false,
                                IsFilled = false
                            };

                            segment = new PolyLineSegment();
                            figure.Segments.Add(segment);
                            figures.Add(figure);
                        }

                        segment.Points.Add(p2);
                    }

                    if (!inside || p2 != points[i])
                    {
                        figure = null;
                    }
                }
            }
        }
    }
}
