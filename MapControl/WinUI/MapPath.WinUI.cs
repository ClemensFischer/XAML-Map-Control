#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using Microsoft.UI.Xaml.Media;
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

        private void SetDataTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform)
            {
                transform.Matrix = matrix;
            }
            else
            {
                Data.Transform = new MatrixTransform { Matrix = matrix };
            }
        }
    }
}
