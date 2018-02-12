// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// A polygon defined by a collection of Locations.
    /// </summary>
    public class MapPolygon : MapShape
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            nameof(Locations), typeof(IEnumerable<Location>), typeof(MapPolygon),
            new PropertyMetadata(null, (o, e) => ((MapPolygon)o).DataCollectionPropertyChanged(e)));

        /// <summary>
        /// Gets or sets the Locations that define the polygon points.
        /// </summary>
        [TypeConverter(typeof(LocationCollectionConverter))]
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected override void UpdateData()
        {
            Data.Figures.Clear();

            if (ParentMap != null && Locations != null && Locations.Count() >= 2)
            {
                var points = Locations.Select(loc => LocationToPoint(loc));
                var polyline = new PolyLineSegment(points.Skip(1), true);

                Data.Figures.Add(new PathFigure(points.First(), new PathSegment[] { polyline }, true));
            }
        }
    }
}
