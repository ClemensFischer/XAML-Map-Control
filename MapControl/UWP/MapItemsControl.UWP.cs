// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.Register(
            nameof(AutoCollapse), typeof(bool), typeof(MapItem),
            new PropertyMetadata(false, (o, e) => MapPanel.SetAutoCollapse((MapItem)o, (bool)e.NewValue)));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapItem),
            new PropertyMetadata(null, (o, e) => MapPanel.SetLocation((MapItem)o, (Location)e.NewValue)));

        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);
            MapPanel.InitMapElement(this);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?.OnItemClicked(
                this, e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control), e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift));
        }
    }

    public partial class MapItemsControl
    {
        public MapItemsControl()
        {
            DefaultStyleKey = typeof(MapItemsControl);
            MapPanel.InitMapElement(this);
        }

        public new FrameworkElement ContainerFromItem(object item)
        {
            return (FrameworkElement)base.ContainerFromItem(item);
        }
    }
}
