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
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape, IWeakEventListener
    {
        public MapPath()
        {
            Stretch = Stretch.None;
        }

        public static readonly DependencyProperty DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty,
                (path, oldValue, newValue) => path.DataPropertyChanged(oldValue, newValue));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry DefiningGeometry => Data;

        private void DataPropertyChanged(Geometry oldData, Geometry newData)
        {
            // Check if data is actually a new Geometry.
            //
            if (newData != null && !ReferenceEquals(newData, oldData))
            {
                if (newData.IsFrozen)
                {
                    Data = newData.Clone(); // DataPropertyChanged called again
                }
                else
                {
                    UpdateData();
                }
            }
        }

        private void SetMapTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform && !transform.IsFrozen)
            {
                transform.Matrix = matrix;
            }
            else
            {
                Data.Transform = new MatrixTransform(matrix);
            }
        }

        #region Methods used only by derived classes MapPolyline, MapPolygon and MapMultiPolygon

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

        protected void SetPathFigures(PathFigureCollection pathFigures)
        {
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

        #endregion
    }
}
