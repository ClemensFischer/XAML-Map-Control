// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if WINDOWS_RUNTIME
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

            if (imageTileSource != null)
            {
                foreach (var tile in tiles)
                {
                    try
                    {
                        var image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                        tile.SetImageSource(image, tileLayer.AnimateTileOpacity);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Loading tile image failed: {0}", ex.Message);
                    }
                }
            }
            else
            {
                foreach (var tile in tiles)
                {
                    try
                    {
                        var uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
                        var image = uri != null ? new BitmapImage(uri) : null;
                        tile.SetImageSource(image, tileLayer.AnimateTileOpacity);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Creating tile image failed: {0}", ex.Message);
                    }
                }
            }
        }

        public void CancelGetTiles()
        {
        }
    }
}
