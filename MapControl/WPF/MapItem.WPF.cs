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

        /// <summary>
        /// Replaces ListBoxItem mouse event handling by not calling base.OnMouseLeftButtonDown.
        /// Setting e.Handled = true generates a MouseLeftButtonUp event in the parent MapItemsControl,
        /// which resembles the behavior of the ListBox base class.
        /// </summary>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                if (ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
                {
                    mapItemsControl.OnItemClicked(this, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
                }
            }
        }

        /// <summary>
        /// Replaces ListBoxItem mouse event handling by not calling base.OnMouseRightButtonDown.
        /// Setting e.Handled = true generates a MouseRightButtonUp event in the parent MapItemsControl,
        /// which resembles the behavior of the ListBox base class.
        /// </summary>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
