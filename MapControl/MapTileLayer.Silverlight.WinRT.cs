// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class MapTileLayer
    {
        partial void Initialize()
        {
            IsHitTestVisible = false;

            MapPanel.AddParentMapHandlers(this);
        }
    }
}
