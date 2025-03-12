﻿using Windows.System;
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
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.Register<MapItem, bool>(nameof(AutoCollapse), false,
                (item, oldValue, newValue) => MapPanel.SetAutoCollapse(item, newValue));

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapItem, Location>(nameof(Location), null,
                (item, oldValue, newValue) =>
                {
                    MapPanel.SetLocation(item, newValue);
                    item.UpdateMapTransform(newValue);
                });

        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);
            MapPanel.InitMapElement(this);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            // Prevent default handling in ListBoxItem by not calling base.OnPointerPressed.
            if (!e.Handled)
            {
                e.Handled = true;
                (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?
                   .OnItemClicked(this, e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control));
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            // Prevent default handling in ListBoxItem by not calling base.OnPointerReleased.
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
