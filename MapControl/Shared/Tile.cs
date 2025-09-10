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
    public partial class Tile
    {
        public Tile(int zoomLevel, int x, int y, int columnCount)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
            Column = ((x % columnCount) + columnCount) % columnCount;
        }

        public Image Image { get; } = new Image { Stretch = Stretch.Fill };
        public int ZoomLevel { get; }
        public int X { get; }
        public int Y { get; }
        public int Column { get; }
        public int Row => Y;
        public bool IsPending { get; set; } = true;
    }
}
