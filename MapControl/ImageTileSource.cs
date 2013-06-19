// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    /// <summary>
    /// Provides the image of a map tile. ImageTileSource bypasses download and
    /// cache processing in TileImageLoader. By overriding the LoadImage method,
    /// an application can provide tile images from an arbitrary source.
    /// If the CanLoadAsync property is true, the LoadImage method will be called
    /// from a separate, non-UI thread and must hence return a frozen ImageSource.
    /// </summary>
    public class ImageTileSource : TileSource
    {
        public virtual bool CanLoadAsync
        {
            get { return false; }
        }

        public virtual ImageSource LoadImage(int x, int y, int zoomLevel)
        {
            return new BitmapImage(GetUri(x, y, zoomLevel));
        }
    }
}
