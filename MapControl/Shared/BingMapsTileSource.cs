// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class BingMapsTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (UriFormat != null && Subdomains != null && Subdomains.Length > 0 && zoomLevel > 0)
            {
                var subdomain = Subdomains[(x + y) % Subdomains.Length];
                var quadkey = new char[zoomLevel];

                for (var z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
                {
                    quadkey[z] = (char)('0' + 2 * (y % 2) + (x % 2));
                }

                uri = new Uri(UriFormat
                    .Replace("{subdomain}", subdomain)
                    .Replace("{quadkey}", new string(quadkey)));
            }

            return uri;
        }
    }
}
