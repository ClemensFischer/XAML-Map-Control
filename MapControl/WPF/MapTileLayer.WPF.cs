// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Markup;

namespace MapControl
{
    [ContentProperty("TileSource")]
    public partial class MapTileLayer
    {
        static MapTileLayer()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(MapTileLayer), new FrameworkPropertyMetadata(false));
        }
    }
}
