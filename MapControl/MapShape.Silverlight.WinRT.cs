// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    public partial class MapShape : Path
    {
        public MapShape(Geometry geometry)
        {
            Data = Geometry = geometry;
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
