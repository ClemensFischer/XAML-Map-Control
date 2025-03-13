namespace MapControl
{
    public partial class MapItem
    {
        public static readonly StyledProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapItem, bool>(MapPanel.AutoCollapseProperty);

        public static readonly StyledProperty<Location> LocationProperty =
            DependencyPropertyHelper.AddOwner<MapItem, Location>(MapPanel.LocationProperty, null,
                (item, oldValue, newValue) => item.UpdateMapTransform(newValue));

        /// <summary>
        /// Replaces ListBoxItem pointer event handling by not calling base.OnPointerPressed.
        /// Setting e.Handled = true generates a PointerReleased event in the parent MapItemsControl,
        /// which resembles the behavior of the ListBox base class.
        /// </summary>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                if (ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
                {
                    mapItemsControl.OnItemClicked(this, e.KeyModifiers.HasFlag(KeyModifiers.Control));
                }
            }
        }
    }
}
