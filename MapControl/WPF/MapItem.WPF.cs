using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            MapPanel.AutoCollapseProperty.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty LocationProperty =
            MapPanel.LocationProperty.AddOwner(typeof(MapItem),
                new FrameworkPropertyMetadata(null, (o, e) => ((MapItem)o).UpdateMapTransform()));

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
