// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// A multi-polygon defined by a collection of collections of Locations.
    /// Allows to draw polygons with holes.
    /// 
    /// A PolygonCollection (with ObservableCollection of Location elements) may be used
    /// for the Polygons property if collection changes of the property itself and its
    /// elements are both supposed to trigger UI updates.
    /// </summary>
    public class MapMultiPolygon : MapPath
    {
        public static readonly DependencyProperty PolygonsProperty =
            DependencyPropertyHelper.Register<MapMultiPolygon, IEnumerable<IEnumerable<Location>>>(nameof(Polygons), null,
                (polygon, oldValue, newValue) => polygon.DataCollectionPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapMultiPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polygon, oldValue, newValue) => ((PathGeometry)polygon.Data).FillRule = newValue);

        /// <summary>
        /// Gets or sets the Locations that define the multi-polygon points.
        /// </summary>
        public IEnumerable<IEnumerable<Location>> Polygons
        {
            get => (IEnumerable<IEnumerable<Location>>)GetValue(PolygonsProperty);
            set => SetValue(PolygonsProperty, value);
        }

        public FillRule FillRule
        {
            get => (FillRule)GetValue(FillRuleProperty);
            set => SetValue(FillRuleProperty, value);
        }

        public MapMultiPolygon()
        {
            Data = new PathGeometry();
        }

        protected override void UpdateData()
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();
            AddMultiPolygonPoints(figures, Polygons);
        }
    }
}
