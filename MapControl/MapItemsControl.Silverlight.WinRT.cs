// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class MapItemsControl
    {
        public MapItemsControl()
        {
            DefaultStyleKey = typeof(MapItemsControl);
            MapPanel.AddParentMapHandlers(this);
        }
    }
}