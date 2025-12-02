using System;

namespace MapControl
{
    public class UriTileSource : TileSource
    {
        private string uriTemplate;
        private string uriFormat;

        public string UriTemplate
        {
            get => uriTemplate;
            set
            {
                uriTemplate = value;
                uriFormat = uriTemplate
                    .Replace("{z}", "{0}")
                    .Replace("{x}", "{1}")
                    .Replace("{y}", "{2}")
                    .Replace("{s}", "{3}");

                if (Subdomains == null && uriTemplate.Contains("{s}"))
                {
                    Subdomains = ["a", "b", "c"]; // default OpenStreetMap subdomains
                }
            }
        }

        public string[] Subdomains { get; set; }

        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            Uri uri = null;

            if (uriFormat != null)
            {
                var uriString = Subdomains?.Length > 0
                    ? string.Format(uriFormat, zoomLevel, column, row, Subdomains[(column + row) % Subdomains.Length])
                    : string.Format(uriFormat, zoomLevel, column, row);

                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }

            return uri;
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
