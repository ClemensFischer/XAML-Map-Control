// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Shape
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(ICollection<Location>), typeof(MapPolyline),
            new PropertyMetadata(null, LocationsPropertyChanged));

        protected override Geometry DefiningGeometry
        {
            get { return Geometry; }
        }
    }
}
