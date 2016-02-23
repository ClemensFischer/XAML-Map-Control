// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    public partial class MapPath : Path
    {
        private Geometry data;

        public MapPath()
        {
            MapPanel.AddParentMapHandlers(this);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (Stretch != Stretch.None)
            {
                Stretch = Stretch.None;
            }

            // Workaround for missing PropertyChangedCallback for the Data property.
            if (data != Data)
            {
                data = Data;
                UpdateData();
            }

            // Path.MeasureOverride in Windows Runtime sometimes returns an empty Size,
            // whereas in Silverlight it occasionally throws an ArgumentException,
            // apparently because it tries to create a Size from negative width or height,
            // which result from a transformed Geometry.
            // In either case it seems to be sufficient to simply return a non-zero size.
            return new Size(1, 1);
        }
    }
}
