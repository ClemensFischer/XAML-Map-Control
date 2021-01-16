// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty LocationProperty = MapPanel.LocationProperty.AddOwner(typeof(MapItem));

        static MapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?.OnItemClicked(
                this, Keyboard.Modifiers.HasFlag(ModifierKeys.Control), Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        }
    }

    public partial class MapItemsControl
    {
        static MapItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
        }

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
