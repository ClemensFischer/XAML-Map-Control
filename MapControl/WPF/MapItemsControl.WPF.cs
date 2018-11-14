// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public class MapItem : ListBoxItem
    {
        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);

            MapPanel.InitMapElement(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;

            (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?.OnItemClicked(
                this, Keyboard.Modifiers.HasFlag(ModifierKeys.Control), Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        }
    }

    public partial class MapItemsControl
    {
        public FrameworkElement ContainerFromItem(object item)
        {
            return (FrameworkElement)ItemContainerGenerator.ContainerFromItem(item);
        }

        public object ItemFromContainer(FrameworkElement container)
        {
            return ItemContainerGenerator.ItemFromContainer(container);
        }

        public void SelectItemsInGeometry(Geometry geometry)
        {
            SelectItemsByPosition(p => geometry.FillContains(p));
        }
    }
}
