// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    public partial class MapPath : Path
    {
        public MapPath()
        {
            MapPanel.InitMapElement(this);
        }

#region Methods used only by derived classes MapPolyline and MapPolygon

        protected void DataCollectionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= DataCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += DataCollectionChanged;
            }

            UpdateData();
        }

        protected void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected void AddPolylineLocations(PathFigureCollection pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            if (locations.Count() >= 2)
            {
                var points = locations.Select(location => LocationToView(location, longitudeOffset));

                if (closed)
                {
                    var segment = new PolyLineSegment();

                    foreach (var point in points.Skip(1))
                    {
                        segment.Points.Add(point);
                    }

                    var figure = new PathFigure
                    {
                        StartPoint = points.First(),
                        IsClosed = closed,
                        IsFilled = closed
                    };

                    figure.Segments.Add(segment);
                    pathFigures.Add(figure);
                }
                else
                {
                    var pointList = points.ToList();

                    if (closed)
                    {
                        pointList.Add(pointList[0]);
                    }

                    var viewport = new Rect(0, 0, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height);
                    PathFigure figure = null;
                    PolyLineSegment segment = null;

                    for (int i = 1; i < pointList.Count; i++)
                    {
                        var p1 = pointList[i - 1];
                        var p2 = pointList[i];
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
                                pathFigures.Add(figure);
                            }

                            segment.Points.Add(p2);
                        }

                        if (!inside || p2 != pointList[i])
                        {
                            figure = null;
                        }
                    }
                }
            }
        }

#endregion
    }
}
