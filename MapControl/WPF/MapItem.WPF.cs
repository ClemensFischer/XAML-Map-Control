using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.AddOwner<MapItem, Location>(MapPanel.LocationProperty,
                (item, oldValue, newValue) => item.UpdateMapTransform());

        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapItem, bool>(MapPanel.AutoCollapseProperty);

        static MapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl &&
                mapItemsControl.SelectionMode == SelectionMode.Extended)
            {
                mapItemsControl.SelectItemsInRange(this);
                e.Handled = true;
            }
            else
            {
                base.OnMouseLeftButtonDown(e);
            }
        }
    }
}
