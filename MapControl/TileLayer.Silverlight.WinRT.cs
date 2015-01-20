// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
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
