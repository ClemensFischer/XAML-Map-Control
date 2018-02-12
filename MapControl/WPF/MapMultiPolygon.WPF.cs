// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// A multi-polygon defined by a collection of collections of Locations.
    /// With a FillRule of EvenOdd, this allows to draw polygons with holes.
    /// 
    /// A PolygonCollection (with ObservableCollection of Location elements) may be used
    /// for the Polygons property if collection changes of the property itself and its
    /// elements are both supposed to trigger a UI update.
    /// </summary>
    public class MapMultiPolygon : MapShape, IWeakEventListener
    {
        public static readonly DependencyProperty PolygonsProperty = DependencyProperty.Register(
            nameof(Polygons), typeof(IEnumerable<IEnumerable<Location>>), typeof(MapMultiPolygon),
            new PropertyMetadata(null, (o, e) => ((MapMultiPolygon)o).DataCollectionPropertyChanged(e)));

        /// <summary>
        /// Gets or sets the Locations that define the multi-polygon points.
        /// </summary>
        public IEnumerable<IEnumerable<Location>> Polygons
        {
            get { return (IEnumerable<IEnumerable<Location>>)GetValue(PolygonsProperty); }
            set { SetValue(PolygonsProperty, value); }
        }

        protected override void UpdateData()
        {
            Data.Figures.Clear();

            if (ParentMap != null && Polygons != null)
            {
                foreach (var polygon in Polygons.Where(p => p.Count() >= 2))
                {
                    var points = polygon.Select(loc => LocationToPoint(loc));
                    var polyline = new PolyLineSegment(points.Skip(1), true);

                    Data.Figures.Add(new PathFigure(points.First(), new PathSegment[] { polyline }, true));
                }
            }
        }
    }
}
