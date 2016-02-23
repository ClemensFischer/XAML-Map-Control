// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the image of a map tile. ImageTileSource bypasses image
    /// downloading  in TileImageLoader. By overriding the LoadImage method,
    /// an application can provide tile images from an arbitrary source.
    /// </summary>
    public class ImageTileSource : TileSource
    {
        public virtual ImageSource LoadImage(int x, int y, int zoomLevel)
        {
            var uri = GetUri(x, y, zoomLevel);

            return uri != null ? new BitmapImage(uri) : null;
        }
    }
}
