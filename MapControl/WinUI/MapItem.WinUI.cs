using System;
using Windows.System;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
#endif

namespace MapControl;

public partial class MapItem
{
    private Windows.Foundation.Point? pointerPressedPosition;

    public MapItem()
    {
        DefaultStyleKey = typeof(MapItem);
        MapPanel.InitMapElement(this);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetCurrentPoint(null);

        if (point.Properties.IsLeftButtonPressed)
        {
            pointerPressedPosition = point.Position;
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        if (pointerPressedPosition.HasValue)
        {
            const double pointerMovementThreshold = 2d;
            var position = e.GetCurrentPoint(null).Position;

            // Perform selection only when no significant pointer movement occured.
            //
            if (Math.Abs(position.X - pointerPressedPosition.Value.X) <= pointerMovementThreshold &&
                Math.Abs(position.Y - pointerPressedPosition.Value.Y) <= pointerMovementThreshold)
            {
                if (ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl &&
                    mapItemsControl.SelectionMode == SelectionMode.Extended &&
                    e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift))
                {
                    mapItemsControl.SelectItemsInRange(this);
                }
                else
                {
                    base.OnPointerReleased(e);
                }
            }

            pointerPressedPosition = null;
        }

        e.Handled = true;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var parentMap = MapPanel.GetParentMap(this);

        if (parentMap != null)
        {
            // Workaround for missing RelativeSource AncestorType=MapBase Bindings in default Style.
            //
            if (Background == null)
            {
                SetBinding(BackgroundProperty,
                    new Binding { Source = parentMap, Path = new PropertyPath(nameof(Background)) });
            }
            if (Foreground == null)
            {
                SetBinding(ForegroundProperty,
                    new Binding { Source = parentMap, Path = new PropertyPath(nameof(Foreground)) });
            }
            if (BorderBrush == null)
            {
                SetBinding(BorderBrushProperty,
                    new Binding { Source = parentMap, Path = new PropertyPath(nameof(Foreground)) });
            }
        }
    }
}
