// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class TileLayer
    {
        partial void Initialize()
        {
            IsHitTestVisible = false;
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
