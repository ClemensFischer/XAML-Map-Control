// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.System;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
                (item, oldValue, newValue) => item.LocationPropertyChanged(newValue));

        private void LocationPropertyChanged(Location location)
        {
            MapPanel.SetLocation(this, location);
            UpdateMapTransform(location);
        }

        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);
            MapPanel.InitMapElement(this);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            (ItemsControl.ItemsControlFromItemContainer(this) as MapItemsControl)?.OnItemClicked(
                this, e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control), e.KeyModifiers.HasFlag(VirtualKeyModifiers.Shift));
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null)
            {
                // If this.Background is not explicitly set, bind it to parentMap.Background
                this.SetBindingOnUnsetProperty(BackgroundProperty, parentMap, Panel.BackgroundProperty, nameof(Background));

                // If this.Foreground is not explicitly set, bind it to parentMap.Foreground
                this.SetBindingOnUnsetProperty(ForegroundProperty, parentMap, MapBase.ForegroundProperty, nameof(Foreground));

                // If this.BorderBrush is not explicitly set, bind it to parentMap.Foreground
                this.SetBindingOnUnsetProperty(BorderBrushProperty, parentMap, MapBase.ForegroundProperty, nameof(Foreground));
            }
        }
    }
}
