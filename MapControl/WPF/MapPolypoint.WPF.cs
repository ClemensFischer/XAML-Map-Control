// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Base class of MapPolyline, MapPolygon and MapMultiPolygon.
    /// </summary>
    public class MapPolypoint : MapPath, IWeakEventListener
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
                CollectionChangedEventManager.RemoveListener(oldCollection, this);
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                CollectionChangedEventManager.AddListener(newCollection, this);
            }

            UpdateData();
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            UpdateData();
            return true;
        }

        protected void UpdateData(IEnumerable<Location> locations, bool closed)
        {
            var pathFigures = new PathFigureCollection();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(pathFigures, locations, longitudeOffset, closed);
            }

            ((PathGeometry)Data).Figures = pathFigures;
        }

        protected void UpdateData(IEnumerable<IEnumerable<Location>> polygons)
        {
            var pathFigures = new PathFigureCollection();

            if (ParentMap != null && polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location);

                foreach (var polygon in polygons)
                {
                    AddPolylinePoints(pathFigures, polygon, longitudeOffset, true);
                }
            }

            ((PathGeometry)Data).Figures = pathFigures;
        }

        protected void AddPolylinePoints(PathFigureCollection pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            if (locations.Count() >= 2)
            {
                var points = locations
                    .Select(location => LocationToView(location, longitudeOffset))
                    .Where(point => point.HasValue)
                    .Select(point => point.Value);

                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = closed,
                    IsFilled = true
                };

                figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
                pathFigures.Add(figure);
            }
        }
    }
}
