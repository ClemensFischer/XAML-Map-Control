// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class of MapPolyline and MapPolygon.
    /// </summary>
    public class MapPolypoint : MapPath
    {
        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polypoint, oldValue, newValue) => ((PathGeometry)polypoint.Data).FillRule = newValue);

        public FillRule FillRule
        {
            get => (FillRule)GetValue(FillRuleProperty);
            set => SetValue(FillRuleProperty, value);
        }

        protected MapPolypoint()
        {
            Data = new PathGeometry();
        }

        protected void DataCollectionPropertyChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= DataCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += DataCollectionChanged;
            }

            UpdateData();
        }

        protected void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected void UpdateData(IEnumerable<Location> locations, bool closed)
        {
            var pathFigures = ((PathGeometry)Data).Figures;
            pathFigures.Clear();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(pathFigures, locations, longitudeOffset, closed);
            }
        }

        private void AddPolylinePoints(PathFigureCollection pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            if (locations.Count() >= 2)
            {
                var points = locations
                    .Select(location => LocationToView(location, longitudeOffset))
                    .Where(point => point.HasValue)
                    .Select(point => point.Value);

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
                        IsFilled = true
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
                                    IsFilled = true
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
    }
}
