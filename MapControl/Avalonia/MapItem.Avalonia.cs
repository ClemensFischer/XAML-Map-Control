namespace MapControl
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
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                MapItemsControl.SetSelectedItemsRange(this);
            }
            else
            {
                base.OnPointerPressed(e);
            }
        }
    }
}
