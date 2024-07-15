// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
            var pathFigure = new PathFigure();

            if (ParentMap != null && locations?.Count() >= 2)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.First());

                AddPolylinePoints(pathFigure, locations, longitudeOffset, closed);
            }

            ((PathGeometry)Data).Figures = new PathFigures { pathFigure };

            InvalidateGeometry(); // ignores an empty PathGeometry.Figures collection
        }

        private void AddPolylinePoints(PathFigure pathFigure, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations
                .Select(location => LocationToView(location, longitudeOffset))
                .Where(point => point.HasValue)
                .Select(point => point.Value);

            pathFigure.StartPoint = points.First();
            pathFigure.Segments.Add(new PolyLineSegment(points.Skip(1)));
            pathFigure.IsClosed = closed;
            pathFigure.IsFilled = true;
        }
    }
}
