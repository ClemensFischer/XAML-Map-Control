// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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
        public static readonly DependencyProperty DataProperty = Path.DataProperty.AddOwner(
            typeof(MapPath), new PropertyMetadata(null, DataPropertyChanged));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry DefiningGeometry => Data;

        private static void DataPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && !ReferenceEquals(e.NewValue, e.OldValue)) // Data is actually a new Geometry
            {
                var path = (MapPath)obj;
                var data = (Geometry)e.NewValue;

                if (data.IsFrozen)
                {
                    path.Data = data.Clone(); // DataPropertyChanged called again
                }
                else
                {
                    data.Transform = new MatrixTransform();
                    path.UpdateData();
                }
            }
        }

        #region Methods used only by derived classes MapPolyline, MapPolygon and MapMultiPolygon

        protected void DataCollectionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                CollectionChangedEventManager.RemoveListener(oldCollection, this);
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
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

        protected void AddPolylineLocations(PathFigureCollection pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            if (locations.Count() >= 2)
            {
                var points = locations.Select(location => LocationToView(location, longitudeOffset));
                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = closed,
                    IsFilled = closed
                };

                figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
                pathFigures.Add(figure);
            }
        }

        #endregion
    }
}
