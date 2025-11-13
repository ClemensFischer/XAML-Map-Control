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
    public class TileSource : ITileSource
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

        public bool Cacheable => UriTemplate != null && UriTemplate.StartsWith("http");

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

        public virtual Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var uri = GetUri(zoomLevel, column, row);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }

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
