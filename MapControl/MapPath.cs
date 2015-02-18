// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map shapes. The shape geometry is given by the Data property,
    /// which must contain a Geometry defined in cartesian (projected) map coordinates.
    /// The Stretch property is meaningless for MapPath, it will be reset to None.
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
    }
}
