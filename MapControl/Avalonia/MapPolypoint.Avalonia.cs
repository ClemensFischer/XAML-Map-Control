// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
            var pathFigures = new PathFigures();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(pathFigures, locations, longitudeOffset, closed);
            }

            if (pathFigures.Count == 0)
            {
                // Avalonia Shape seems to ignore PathGeometry with empty Figures collection

                pathFigures.Add(new PathFigure { StartPoint = new Point(-1000, -1000) });
            }

            ((PathGeometry)Data).Figures = pathFigures;

            InvalidateGeometry();
        }

        private void AddPolylinePoints(PathFigures pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations
                .Select(location => LocationToView(location, longitudeOffset))
                .Where(point => point.HasValue)
                .Select(point => point.Value);

            if (points.Any())
            {
                var startPoint = points.First();
                var polylineSegment = new PolyLineSegment(points.Skip(1));
                var minX = startPoint.X;
                var maxX = startPoint.X;
                var minY = startPoint.Y;
                var maxY = startPoint.Y;

                foreach (var point in polylineSegment.Points)
                {
                    minX = Math.Min(minX, point.X);
                    maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxY = Math.Max(maxY, point.Y);
                }

                if (maxX >= 0 && minX <= ParentMap.ActualWidth &&
                    maxY >= 0 && minY <= ParentMap.ActualHeight)
                {
                    var pathFigure = new PathFigure
                    {
                        StartPoint = startPoint,
                        IsClosed = closed,
                        IsFilled = true
                    };

                    pathFigure.Segments.Add(polylineSegment);
                    pathFigures.Add(pathFigure);
                }
            }
        }
    }
}
