// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
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

        internal void BeginGetTiles(IEnumerable<Tile> tiles)
        {
            foreach (var tile in tiles.Where(t => t.ImageSource == null && t.Uri == null))
            {
                tile.Uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
                tile.SetImageSource(new BitmapImage(tile.Uri), true);
            }
        }

        internal void CancelGetTiles()
        {
        }
    }
}
