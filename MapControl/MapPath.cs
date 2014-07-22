// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map shapes.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
        }

        void IMapElement.SetParentMap(MapBase map)
        {
            parentMap = map;
            UpdateData();
        }

        protected virtual void UpdateData()
        {
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
