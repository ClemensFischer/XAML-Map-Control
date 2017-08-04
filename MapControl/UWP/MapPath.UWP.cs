// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public partial class MapPath : Path
    {
        private Geometry data;

        public MapPath()
        {
            MapPanel.AddParentMapHandlers(this);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Stretch != Stretch.None)
            {
                Stretch = Stretch.None;
            }

            // Workaround for missing PropertyChangedCallback for the Data property.
            if (data != Data)
            {
                if (data != null)
                {
                    data.ClearValue(Geometry.TransformProperty);
                }

                data = Data;

                if (data != null)
                {
                    data.Transform = viewportTransform;
                }
            }

            return new Size();
        }
    }
}
