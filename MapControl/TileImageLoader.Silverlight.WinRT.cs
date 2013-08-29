// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    /// <summary>
    /// Loads map tile images by their URIs.
    /// </summary>
    internal class TileImageLoader
    {
        private readonly TileLayer tileLayer;

        internal TileImageLoader(TileLayer tileLayer)
        {
            this.tileLayer = tileLayer;
        }

        internal void StartGetTiles(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                if (uri != null)
                {
                    tile.SetImageSource(new BitmapImage(uri), tileLayer.AnimateTileOpacity);
                }
            }
        }

        internal void CancelGetTiles()
        {
        }
    }
}
