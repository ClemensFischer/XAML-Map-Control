// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
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
            set
            {
                parentMap = value;
                UpdateData();
            }
        }

        protected virtual void UpdateData()
        {
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // base.MeasureOverride in WPF and WinRT sometimes return a Size with zero
            // width or height, whereas base.MeasureOverride in Silverlight occasionally
            // throws an ArgumentException, as it tries to create a Size from a negative
            // width or height, apparently resulting from a transformed Geometry.
            // In either case it seems to be sufficient to simply return a non-zero size.
            return new Size(1, 1);
        }
    }
}
