// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
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
        public static readonly DependencyProperty PolygonsProperty = DependencyProperty.Register(
            nameof(Polygons), typeof(IEnumerable<IEnumerable<Location>>), typeof(MapMultiPolygon),
            new PropertyMetadata(null, (o, e) => ((MapMultiPolygon)o).DataCollectionPropertyChanged(e)));

        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            nameof(FillRule), typeof(FillRule), typeof(MapMultiPolygon),
            new PropertyMetadata(FillRule.EvenOdd, (o, e) => ((PathGeometry)((MapMultiPolygon)o).Data).FillRule = (FillRule)e.NewValue));

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
            var pathFigures = ((PathGeometry)Data).Figures;
            pathFigures.Clear();

            if (ParentMap != null && Polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location);

                foreach (var polygon in Polygons)
                {
                    AddPolylineLocations(pathFigures, polygon, longitudeOffset, true);
                }
            }
        }
    }
}
