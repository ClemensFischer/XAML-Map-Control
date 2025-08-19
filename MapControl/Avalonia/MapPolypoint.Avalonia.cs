using Avalonia;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MapControl
{
    /// <summary>
    /// Base class of MapPolyline and MapPolygon.
    /// </summary>
    public class MapPolypoint : MapPath
    {
        public static readonly StyledProperty<FillRule> FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polypoint, oldValue, newValue) => ((PathGeometry)polypoint.Data).FillRule = newValue);

        public FillRule FillRule
        {
            get => GetValue(FillRuleProperty);
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
            var figures = new PathFigures();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(figures, locations, longitudeOffset, closed);
            }

            SetPathFigures(figures);
        }

        protected void UpdateData(IEnumerable<IEnumerable<Location>> polygons)
        {
            var figures = new PathFigures();

            if (ParentMap != null && polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location);

                foreach (var locations in polygons)
                {
                    AddPolylinePoints(figures, locations, longitudeOffset, true);
                }
            }

            SetPathFigures(figures);
        }

        private void AddPolylinePoints(PathFigures figures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations
                .Select(location => LocationToView(location, longitudeOffset))
                .Where(point => point.HasValue)
                .Select(point => point.Value);

            if (points.Any())
            {
                var start = points.First();
                var polyline = new PolyLineSegment(points.Skip(1));
                var minX = start.X;
                var maxX = start.X;
                var minY = start.Y;
                var maxY = start.Y;

                foreach (var point in polyline.Points)
                {
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

        private void SetPathFigures(PathFigures figures)
        {
            if (figures.Count == 0)
            {
                // Avalonia Shape seems to ignore PathGeometry with empty Figures collection

                figures.Add(new PathFigure { StartPoint = new Point(-1000, -1000) });
            }

            ((PathGeometry)Data).Figures = figures;
            InvalidateGeometry();
        }
    }
}
