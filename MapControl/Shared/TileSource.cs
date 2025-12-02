using System;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the download Uri or ImageSource of map tiles. Used by TileImageLoader.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(TileSourceConverter))]
#endif
    public class TileSource
    {
        /// <summary>
        /// Gets an image request Uri for the specified zoom level and tile indices.
        /// May return null when the image shall be loaded by
        /// the LoadImageAsync(zoomLevel, column, row) method.
        /// </summary>
        public virtual Uri GetUri(int zoomLevel, int column, int row)
        {
            return null;
        }

        /// <summary>
        /// Loads a tile image without an Uri.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            return null;
        }

        /// <summary>
        /// Loads a tile image from an Uri.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(Uri uri)
        {
            return ImageLoader.LoadImageAsync(uri);
        }

        /// <summary>
        /// Loads a tile image from an encoded frame buffer.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return ImageLoader.LoadImageAsync(buffer);
        }

        /// <summary>
        /// Creates a TileSource instance from an Uri template string.
        /// </summary>
        public static TileSource Parse(string uriTemplate)
        {
            return new UriTileSource(uriTemplate);
        }
    }
}
