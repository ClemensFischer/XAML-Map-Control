// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapShape : Shape
    {
        public MapShape(Geometry geometry)
        {
            Geometry = geometry;
        }

        protected override Geometry DefiningGeometry
        {
            get { return Geometry; }
        }
    }
}
