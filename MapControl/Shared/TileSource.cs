using System;
using System.Text;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
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
    public class TileSource
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
                    Subdomains = new string[] { "a", "b", "c" }; // default OpenStreetMap subdomains
                }
            }
        }

        /// <summary>
        /// Gets or sets an array of request subdomain names that are replaced for the {s} format specifier.
        /// </summary>
        public string[] Subdomains { get; set; }

        /// <summary>
        /// Gets the image Uri for the specified tile indices and zoom level.
        /// </summary>
        public virtual Uri GetUri(int zoomLevel, int column, int row)
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

        /// <summary>
        /// Loads a tile ImageSource asynchronously from GetUri(zoomLevel, column, row).
        /// This method is called by TileImageLoader when caching is disabled.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var uri = GetUri(zoomLevel, column, row);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }

        /// <summary>
        /// Loads a tile ImageSource asynchronously from an encoded frame buffer in a byte array.
        /// This method is called by TileImageLoader when caching is enabled.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return ImageLoader.LoadImageAsync(buffer);
        }

        public override string ToString()
        {
            return UriTemplate;
        }

        /// <summary>
        /// Creates a TileSource instance from an Uri template string.
        /// </summary>
        public static TileSource Parse(string uriTemplate)
        {
            return new TileSource { UriTemplate = uriTemplate };
        }
    }

    public class TmsTileSource : TileSource
    {
        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            return base.GetUri(zoomLevel, column, (1 << zoomLevel) - 1 - row);
        }
    }
}
