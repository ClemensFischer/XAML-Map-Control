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

namespace MapControl
{
    public partial class MapItem
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapItem, Location>(nameof(Location), null,
                (item, oldValue, newValue) =>
                {
                    MapPanel.SetLocation(item, newValue);
                    item.UpdateMapTransform();
                });

        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.Register<MapItem, bool>(nameof(AutoCollapse), false,
                (item, oldValue, newValue) => MapPanel.SetAutoCollapse(item, newValue));

        private Windows.Foundation.Point? pointerPressedPosition;

        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);
            MapPanel.InitMapElement(this);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            pointerPressedPosition = e.GetCurrentPoint(null).Position;
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if (pointerPressedPosition.HasValue)
            {
                const float pointerMovementThreshold = 2f;
                var p = e.GetCurrentPoint(null).Position;

                // Perform selection only when no significant pointer movement occured.
                //
                if (Math.Abs(p.X - pointerPressedPosition.Value.X) <= pointerMovementThreshold &&
                    Math.Abs(p.Y - pointerPressedPosition.Value.Y) <= pointerMovementThreshold &&
                    ItemsControl.ItemsControlFromItemContainer(this) is MapItemsControl mapItemsControl)
                {
                    if (mapItemsControl.SelectionMode == SelectionMode.Extended &&
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
}
