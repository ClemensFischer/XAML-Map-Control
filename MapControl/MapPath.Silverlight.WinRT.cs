// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml.Shapes;
#else
using System.Windows.Shapes;
#endif

namespace MapControl
{
    public partial class MapPath : Path
    {
        public MapPath()
        {
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
