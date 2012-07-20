using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SampleApplication
{
    public class ImageTileSource : MapControl.ImageTileSource
    {
        public override ImageSource GetImage(int x, int y, int zoomLevel)
        {
            return new BitmapImage(GetUri(x, y, zoomLevel));
        }
    }
}
