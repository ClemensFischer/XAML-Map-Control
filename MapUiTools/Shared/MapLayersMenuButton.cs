// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl.UiTools
{
    public class MapLayersMenuButton : MenuButton
    {
        public MapLayersMenuButton()
            : base("\uE81E")
        {
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            nameof(Map), typeof(MapBase), typeof(MapLayersMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapLayersMenuButton)o).InitializeMenu()));

        public static readonly DependencyProperty MapLayersProperty = DependencyProperty.Register(
            nameof(MapLayers), typeof(IDictionary<string, UIElement>), typeof(MapLayersMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapLayersMenuButton)o).InitializeMenu()));

        public static readonly DependencyProperty MapOverlaysProperty = DependencyProperty.Register(
            nameof(MapOverlays), typeof(IDictionary<string, UIElement>), typeof(MapLayersMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapLayersMenuButton)o).InitializeMenu()));

        public MapBase Map
        {
            get { return (MapBase)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        public IDictionary<string, UIElement> MapLayers
        {
            get { return (IDictionary<string, UIElement>)GetValue(MapLayersProperty); }
            set { SetValue(MapLayersProperty, value); }
        }

        public IDictionary<string, UIElement> MapOverlays
        {
            get { return (IDictionary<string, UIElement>)GetValue(MapOverlaysProperty); }
            set { SetValue(MapOverlaysProperty, value); }
        }

        private void InitializeMenu()
        {
            if (Map != null && MapLayers != null)
            {
                var menu = CreateMenu();

                foreach (var layer in MapLayers)
                {
                    menu.Items.Add(CreateMenuItem(layer.Key, layer.Value, MapLayerClicked));
                }

                var initialLayer = MapLayers.Values.FirstOrDefault();

                if (MapOverlays != null && MapOverlays.Any())
                {
                    if (initialLayer != null)
                    {
                        menu.Items.Add(CreateSeparator());
                    }

                    foreach (var overlay in MapOverlays)
                    {
                        menu.Items.Add(CreateMenuItem(overlay.Key, overlay.Value, MapOverlayClicked));
                    }
                }

                if (initialLayer != null)
                {
                    SetMapLayer(initialLayer);
                }
            }
        }

        private void MapLayerClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var layer = (UIElement)item.Tag;

            SetMapLayer(layer);
        }

        private void MapOverlayClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var layer = (UIElement)item.Tag;

            ToggleMapOverlay(layer);
        }

        private void SetMapLayer(UIElement layer)
        {
            Map.MapLayer = layer;

            UpdateCheckedStates();
        }

        private void ToggleMapOverlay(UIElement layer)
        {
            if (Map.Children.Contains(layer))
            {
                Map.Children.Remove(layer);
            }
            else
            {
                int index = 1;

                foreach (var overlay in MapOverlays.Values)
                {
                    if (overlay == layer)
                    {
                        Map.Children.Insert(index, layer);
                        break;
                    }

                    if (Map.Children.Contains(overlay))
                    {
                        index++;
                    }
                }
            }

            UpdateCheckedStates();
        }

        private void UpdateCheckedStates()
        {
            foreach (var item in GetMenuItems())
            {
                item.IsChecked = Map.Children.Contains((UIElement)item.Tag);
            }
        }
    }
}
