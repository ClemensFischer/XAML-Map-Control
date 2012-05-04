// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    /// <summary>
    /// A closed map polygon, defined by a collection of geographic locations in the Locations property.
    /// </summary>
    public class MapPolygon : MapPolyline
    {
        public static readonly DependencyProperty FillProperty = Shape.FillProperty.AddOwner(
            typeof(MapPolygon), new FrameworkPropertyMetadata((o, e) => ((MapPolygon)o).drawing.Brush = (Brush)e.NewValue));

        public MapPolygon()
        {
            drawing.Brush = Fill;
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        protected override void UpdateGeometry()
        {
            UpdateGeometry(true);
        }
    }
}
