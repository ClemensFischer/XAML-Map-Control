using System;
using System.Text;
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
    /// Provides the download Uri or ImageSource of map tiles.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(TileSourceConverter))]
#endif
    public abstract class TileSource
    {
        /// <summary>
        /// Indicates whether tile images from this source should be cached.
        /// </summary>
        public abstract bool Cacheable { get; }

        /// <summary>
        /// Gets the image Uri for the specified zoom level and tile indices.
        /// </summary>
        public abstract Uri GetUri(int zoomLevel, int column, int row);

        /// <summary>
        /// Loads a tile image whithout caching.
        /// </summary>
        public abstract Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row);

        /// <summary>
        /// Loads a cacheable tile image from an encoded frame buffer.
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
            return new UriTileSource { UriTemplate = uriTemplate };
        }
    }

    public class UriTileSource : TileSource
    {
        private string uriTemplate;

        /// <summary>
        /// Gets or sets the template string for tile request Uris.
        /// </summary>
        public string UriTemplate
        {
            get => uriTemplate;
            set
            {
                uriTemplate = value;

                if (uriTemplate != null && uriTemplate.Contains("{s}") && Subdomains == null)
                {
                    Subdomains = ["a", "b", "c"]; // default OpenStreetMap subdomains
                }
            }
        }

        public string[] Subdomains { get; set; }

        public override bool Cacheable => UriTemplate != null && UriTemplate.StartsWith("http");

        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            Uri uri = null;

            if (UriTemplate != null)
            {
                var uriBuilder = new StringBuilder(UriTemplate);

                uriBuilder.Replace("{z}", zoomLevel.ToString());
                uriBuilder.Replace("{x}", column.ToString());
                uriBuilder.Replace("{y}", row.ToString());

                if (Subdomains != null && Subdomains.Length > 0)
                {
                    uriBuilder.Replace("{s}", Subdomains[(column + row) % Subdomains.Length]);
                }

                uri = new Uri(uriBuilder.ToString(), UriKind.RelativeOrAbsolute);
            }

            return uri;
        }

        public override Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var uri = GetUri(zoomLevel, column, row);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }

        public override string ToString()
        {
            return UriTemplate;
        }
    }

    public class TmsTileSource : UriTileSource
    {
        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            return base.GetUri(zoomLevel, column, (1 << zoomLevel) - 1 - row);
        }
    }
}
