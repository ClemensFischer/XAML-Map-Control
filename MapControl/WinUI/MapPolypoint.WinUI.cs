using System;
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
    public partial class MapPolypoint : MapPath
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
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(figures, locations, longitudeOffset, closed);
            }
        }

        protected void UpdateData(IEnumerable<IEnumerable<Location>> polygons)
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null && polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location);

                foreach (var locations in polygons)
                {
                    AddPolylinePoints(figures, locations, longitudeOffset, true);
                }
            }
        }

        private void AddPolylinePoints(PathFigureCollection figures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = LocationsToView(locations, longitudeOffset);

            if (points.Any())
            {
                var start = points.First();
                var polyline = new PolyLineSegment();
                var minX = start.X;
                var maxX = start.X;
                var minY = start.Y;
                var maxY = start.Y;

                foreach (var point in points.Skip(1))
                {
                    polyline.Points.Add(point);
                    minX = Math.Min(minX, point.X);
                    maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxY = Math.Max(maxY, point.Y);
                }

                if (maxX >= 0 && minX <= ParentMap.ActualWidth &&
                    maxY >= 0 && minY <= ParentMap.ActualHeight)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = start,
                        IsClosed = closed,
                        IsFilled = true
                    };

                    figure.Segments.Add(polyline);
                    figures.Add(figure);
                }
            }
        }
    }
}
