#if WPF
using System.Windows.Controls;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
#endif

namespace MapControl
{
    public partial class Tile(int zoomLevel, int x, int y, int columnCount)
    {
        public int ZoomLevel { get; } = zoomLevel;
        public int X { get; } = x;
        public int Y { get; } = y;
        public int Column { get; } = ((x % columnCount) + columnCount) % columnCount;
        public int Row => Y;

        public Image Image { get; } = new Image { Stretch = Stretch.Fill };
        public bool IsPending { get; set; } = true;
    }
}
