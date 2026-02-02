#if UWP
using Windows.UI.Xaml.Shapes;
#else
using Microsoft.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    public partial class MapPath : Path
    {
        public MapPath()
        {
            MapPanel.InitMapElement(this);
        }
    }
}
