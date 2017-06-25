// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
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
