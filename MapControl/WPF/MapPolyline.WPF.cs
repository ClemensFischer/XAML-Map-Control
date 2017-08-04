// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapPolyline
    {
        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            nameof(FillRule), typeof(FillRule), typeof(MapPolyline), new FrameworkPropertyMetadata(
                FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender,
                (o, e) => ((StreamGeometry)((MapPolyline)o).Data).FillRule = (FillRule)e.NewValue));

        public MapPolyline()
        {
            Data = new StreamGeometry();
        }

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
        [TypeConverter(typeof(LocationCollectionConverter))]
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected override void UpdateData()
        {
            var geometry = (StreamGeometry)Data;

            if (ParentMap != null && Locations != null && Locations.Any())
            {
                using (var context = geometry.Open())
                {
                    var points = Locations.Select(l => ParentMap.MapProjection.LocationToPoint(l));

                    context.BeginFigure(points.First(), IsClosed, IsClosed);
                    context.PolyLineTo(points.Skip(1).ToList(), true, false);
                }
            }
            else
            {
                geometry.Clear();
            }
        }
    }
}
