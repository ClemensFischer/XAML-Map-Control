// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class BingMapsTileSource : TileSource
    {
        public override Uri GetUri(int column, int row, int zoomLevel)
        {
            Uri uri = null;

            if (UriTemplate != null && Subdomains != null && Subdomains.Length > 0 && zoomLevel > 0)
            {
                var subdomain = Subdomains[(column + row) % Subdomains.Length];
                var quadkey = new char[zoomLevel];

                for (var z = zoomLevel - 1; z >= 0; z--, column /= 2, row /= 2)
                {
                    quadkey[z] = (char)('0' + 2 * (row % 2) + (column % 2));
                }

                uri = new Uri(UriTemplate
                    .Replace("{subdomain}", subdomain)
                    .Replace("{quadkey}", new string(quadkey)));
            }

            return uri;
        }
    }
}
