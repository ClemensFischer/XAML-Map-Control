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
        public int ZoomLevel => zoomLevel;
        public int X => x;
        public int Y => y;
        public int Row => y;
        public int Column { get; } = ((x % columnCount) + columnCount) % columnCount;

        public bool IsPending { get; set; } = true;

        /// <summary>
        /// Runs a tile image download Task and passes the result to the UI thread.
        /// </summary>
        public abstract Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc);
    }
}
