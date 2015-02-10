// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map shapes. The shape geometry is given by the Data property,
    /// which must contain a Geometry defined in cartesian (projected) map coordinates.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                parentMap = value;
                UpdateData();
            }
        }

        protected virtual void UpdateData()
        {
            if (Data != null)
            {
                if (parentMap != null)
                {
                    Data.Transform = ParentMap.ViewportTransform;
                }
                else
                {
                    Data.ClearValue(Geometry.TransformProperty);
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // base.MeasureOverride in WPF and Windows Runtime sometimes return a Size
            // with zero width or height, whereas in Silverlight it occasionally throws
            // an ArgumentException, as it tries to create a Size from a negative width
            // or height, apparently resulting from a transformed Geometry.
            // In either case it seems to be sufficient to simply return a non-zero size.
            return new Size(1, 1);
        }
    }
}
