// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class MapPolyline
    {
        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            nameof(FillRule), typeof(FillRule), typeof(MapPolyline),
            new PropertyMetadata(FillRule.EvenOdd, (o, e) => ((PathGeometry)((MapPolyline)o).Data).FillRule = (FillRule)e.NewValue));


        public MapPolyline()
        {
            Data = new PathGeometry();
        }

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected override void UpdateData()
        {
            var geometry = (PathGeometry)Data;
            geometry.Figures.Clear();

            if (ParentMap != null && Locations != null && Locations.Any())
            {
                var points = Locations.Select(l => ParentMap.MapProjection.LocationToPoint(l));

                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = IsClosed,
                    IsFilled = IsClosed
                };

                var segment = new PolyLineSegment();

                foreach (var point in points.Skip(1))
                {
                    segment.Points.Add(point);
                }

                figure.Segments.Add(segment);
                geometry.Figures.Add(figure);
            }
        }
    }
}
