// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    internal class BingMapsTileSource : TileSource
    {
        private readonly string[] subdomains;

        public BingMapsTileSource(string uriFormat, string[] subdomains)
            : base(uriFormat)
        {
            this.subdomains = subdomains;
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel < 1)
            {
                return null;
            }

            var subdomain = subdomains[(x + y) % subdomains.Length];
            var quadkey = new char[zoomLevel];

            for (var z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
            {
                quadkey[z] = (char)('0' + 2 * (y % 2) + (x % 2));
            }

            return new Uri(UriFormat
                .Replace("{subdomain}", subdomain)
                .Replace("{quadkey}", new string(quadkey)));
        }
    }
}
