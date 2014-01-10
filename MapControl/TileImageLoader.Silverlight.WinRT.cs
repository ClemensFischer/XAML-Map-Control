// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    /// <summary>
    /// Loads map tile images.
    /// </summary>
    internal class TileImageLoader
    {
        internal void BeginGetTiles(TileLayer tileLayer, IEnumerable<Tile> tiles)
        {
            var imageTileSource = tileLayer.TileSource as ImageTileSource;

            foreach (var tile in tiles)
            {
                try
                {
                    ImageSource image;

                    if (imageTileSource != null)
                    {
                        image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                    }
                    else
                    {
                        var uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                        image = uri != null ? new BitmapImage(uri) : null;
                    }

                    tile.SetImageSource(image, tileLayer.AnimateTileOpacity);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Creating tile image failed: {0}", ex.Message);
                }
            }
        }

        internal void CancelGetTiles()
        {
        }
    }
}
