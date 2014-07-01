// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    /// <summary>
    /// Provides the image of a map tile. ImageTileSource bypasses image downloading
    /// and optional caching in TileImageLoader. By overriding the LoadImage method,
    /// an application can provide tile images from an arbitrary source.
    /// If the IsAsync property is true, LoadImage will be called from a separate,
    /// non-UI thread and must therefore return a frozen ImageSource.
    /// </summary>
    public class ImageTileSource : TileSource
    {
        public bool IsAsync { get; set; }

        public virtual ImageSource LoadImage(int x, int y, int zoomLevel)
        {
            ImageSource image = null;

            var uri = GetUri(x, y, zoomLevel);

            if (uri != null)
            {
                if (IsAsync)
                {
                    var buffer = new WebClient().DownloadData(uri);

                    if (buffer != null)
                    {
                        using (var stream = new MemoryStream(buffer))
                        {
                            image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            image.Freeze();
                        }
                    }
                }
                else
                {
                    image = BitmapFrame.Create(uri);
                }
            }

            return image;
        }
    }
}
