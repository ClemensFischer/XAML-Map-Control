using Avalonia.Controls;
using Avalonia.Input;

namespace MapControl
{
    public partial class MapItem
    {
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Pointer.Type != PointerType.Mouse &&
                ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
            {
                mapItemsControl.UpdateSelection(this, e);
            }

            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse &&
                e.InitialPressMouseButton == MouseButton.Left &&
                ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
            {
                mapItemsControl.UpdateSelection(this, e);
            }

            e.Handled = true;
        }
    }
}
