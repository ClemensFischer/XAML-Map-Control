// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map. Uses MapItem as container for items
    /// and (for WPF only) updates the IsCurrent attached property on each MapItem when the
    /// Items.CurrentItem property changes.
    /// </summary>
    public partial class MapItemsControl : ListBox
    {
        public UIElement ContainerFromItem(object item)
        {
            return item != null ? ItemContainerGenerator.ContainerFromItem(item) as UIElement : null;
        }

        public object ItemFromContainer(DependencyObject container)
        {
            return container != null ? ItemContainerGenerator.ItemFromContainer(container) : null;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }
    }
}
