using System;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl
{
    public abstract class Tile(int zoomLevel, int x, int y, int columnCount)
    {
        public int ZoomLevel { get; } = zoomLevel;
        public int X { get; } = x;
        public int Y { get; } = y;
        public int Column { get; } = ((x % columnCount) + columnCount) % columnCount;
        public int Row => Y;

        public bool IsPending { get; set; } = true;

        /// <summary>
        /// Runs a tile image download Task and passes the result to the UI thread.
        /// </summary>
        public abstract Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc);
    }
}
