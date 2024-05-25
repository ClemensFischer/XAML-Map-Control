// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public MapPath()
        {
            Stretch = Stretch.None;
        }

        public static readonly StyledProperty<Geometry> DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty,
                (path, oldValue, newValue) => path.DataPropertyChanged(oldValue, newValue));

        public Geometry Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry CreateDefiningGeometry() => Data;

        private void DataPropertyChanged(Geometry oldData, Geometry newData)
        {
            // Check if data is actually a new Geometry.
            //
            if (newData != null && !ReferenceEquals(newData, oldData))
            {
                UpdateData();
            }
        }

        private void SetMapTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform)
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

        protected void AddPolylinePoints(PathFigures pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
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

                figure.Segments.Add(new PolyLineSegment(points.Skip(1)));
                pathFigures.Add(figure);
            }
        }

        #endregion
    }
}
