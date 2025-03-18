﻿using Windows.System;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
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

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            // In contrast to WPF and Avalonia, item selection is done on PointerReleased.
            //
            if (e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift))
            {
                e.Handled = true;
                MapItemsControl.SelectItemsInRange(this);
            }
            else
            {
                base.OnPointerReleased(e);
            }
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
