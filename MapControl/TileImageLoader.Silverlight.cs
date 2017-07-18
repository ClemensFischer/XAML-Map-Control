// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images.
    /// </summary>
    public class TileImageLoader : ITileImageLoader
    {
        public void LoadTiles(MapTileLayer tileLayer)
        {
            var tileSource = tileLayer.TileSource;
            var imageTileSource = tileSource as ImageTileSource;

            foreach (var tile in tileLayer.Tiles.Where(t => t.Pending))
            {
                tile.Pending = false;

                try
                {
                    ImageSource image = null;
                    Uri uri;

                    if (imageTileSource != null)
                    {
                        image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                    }
                    else if ((uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel)) != null)
                    {
                        image = new BitmapImage(uri);
                    }

                    if (image != null)
                    {
                        tile.SetImage(image);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }
        }
    }
}
