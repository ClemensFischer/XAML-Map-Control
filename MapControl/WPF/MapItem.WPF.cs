using System.Windows;
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
            e.Handled = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
            {
                if (mapItemsControl.SelectionMode == SelectionMode.Extended &&
                    Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    mapItemsControl.SelectItemsInRange(this);
                }
                else
                {
                    // Perform default mouse down item selection on mouse up.
                    //
                    base.OnMouseLeftButtonDown(e);
                }
            }

            e.Handled = true;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            OnMouseLeftButtonUp(e);
        }
    }
}
