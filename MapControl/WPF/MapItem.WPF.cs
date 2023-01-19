// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner(
            typeof(MapItem));

        public static readonly DependencyProperty LocationProperty = MapPanel.LocationProperty.AddOwner(
            typeof(MapItem), new FrameworkPropertyMetadata(null,
                (o, e) => ((MapItem)o).UpdateMapTransform((Location)e.NewValue)));

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
}
