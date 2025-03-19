﻿namespace MapControl
{
    public partial class MapItem
    {
        public static readonly StyledProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapItem, bool>(MapPanel.AutoCollapseProperty);

        public static readonly StyledProperty<Location> LocationProperty =
            DependencyPropertyHelper.AddOwner<MapItem, Location>(MapPanel.LocationProperty, null,
                (item, oldValue, newValue) => item.UpdateMapTransform(newValue));

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Touch)
            {
                UpdateSelection(e);
            }

            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            UpdateSelection(e);

            e.Handled = true;
        }

        private void UpdateSelection(PointerEventArgs e)
        {
            if (ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
            {
                mapItemsControl.UpdateSelection(this,
                    e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    e.KeyModifiers.HasFlag(KeyModifiers.Shift));
            }
        }
    }
}
