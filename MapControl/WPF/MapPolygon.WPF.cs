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

        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            nameof(FillRule), typeof(FillRule), typeof(MapPolygon),
            new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender,
                (o, e) => ((PathGeometry)((MapPolygon)o).Data).FillRule = (FillRule)e.NewValue));

        public MapPolygon()
            : base(new PathGeometry())
        {
        }

        /// <summary>
        /// Gets or sets the Locations that define the polygon points.
        /// </summary>
        [TypeConverter(typeof(LocationCollectionConverter))]
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the FillRule of the PathGeometry that represents the polyline.
        /// </summary>
        public FillRule FillRule
        {
            get { return (FillRule)GetValue(FillRuleProperty); }
            set { SetValue(FillRuleProperty, value); }
        }

        protected override void UpdateData()
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null && Locations != null && Locations.Count() >= 2)
            {
                var points = Locations.Select(loc => LocationToPoint(loc));
                var polyline = new PolyLineSegment(points.Skip(1), true);

                figures.Add(new PathFigure(points.First(), new PathSegment[] { polyline }, true));
            }
        }
    }
}
