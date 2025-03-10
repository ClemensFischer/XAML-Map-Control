﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapItem, bool>(MapPanel.AutoCollapseProperty);

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.AddOwner<MapItem, Location>(MapPanel.LocationProperty, null,
                (item, oldValue, newValue) => item.UpdateMapTransform(newValue));

        static MapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?
                .OnItemClicked(this, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
        }
    }
}
