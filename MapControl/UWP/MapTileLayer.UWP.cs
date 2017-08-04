// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml.Markup;

namespace MapControl
{
    [ContentProperty(Name = "TileSource")]
    public partial class MapTileLayer
    {
        partial void Initialize()
        {
            IsHitTestVisible = false;

            MapPanel.AddParentMapHandlers(this);
        }
    }
}
