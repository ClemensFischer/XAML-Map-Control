// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Provides the image of a map tile.
    /// ImageTileSource bypasses image downloading and optional caching in TileImageLoader.
    /// By overriding the LoadImage method, an application can provide tile images from an arbitrary source.
    /// LoadImage will be called from a non-UI thread and must therefore return a frozen ImageSource.
    /// </summary>
    public class ImageTileSource : TileSource
    {
        public virtual ImageSource LoadImage(int x, int y, int zoomLevel)
        {
            var uri = GetUri(x, y, zoomLevel);

            return uri != null ? BitmapSourceHelper.FromUri(uri) : null;
        }
    }
}
