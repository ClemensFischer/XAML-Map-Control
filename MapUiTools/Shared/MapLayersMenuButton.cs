// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Markup;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
#elif AVALONIA
using Avalonia.Interactivity;
using Avalonia.Metadata;
using DependencyProperty = Avalonia.AvaloniaProperty;
using FrameworkElement = Avalonia.Controls.Control;
#endif

namespace MapControl.UiTools
{
#if WPF
    [ContentProperty(nameof(Layer))]
#elif UWP || WINUI
    [ContentProperty(Name = nameof(Layer))]
#endif
    public class MapLayerItem
    {
#if AVALONIA
        [Content]
#endif
        public FrameworkElement Layer { get; set; }

        public string Text { get; set; }

        public Func<Task<FrameworkElement>> LayerFactory { get; set; }

        public async Task<FrameworkElement> GetLayer() => Layer ?? (Layer = await LayerFactory?.Invoke());
    }

#if WPF
    [ContentProperty(nameof(MapLayers))]
#elif UWP || WINUI
    [ContentProperty(Name = nameof(MapLayers))]
#endif
    public class MapLayersMenuButton : MenuButton
    {
        private FrameworkElement selectedLayer;

        public MapLayersMenuButton()
            : base("\uE81E")
        {
            ((INotifyCollectionChanged)MapLayers).CollectionChanged += async (s, e) => await InitializeMenu();
            ((INotifyCollectionChanged)MapOverlays).CollectionChanged += async (s, e) => await InitializeMenu();
        }

        public static readonly DependencyProperty MapProperty =
            DependencyPropertyHelper.Register<MapLayersMenuButton, MapBase>(nameof(Map), null,
                async (button, oldValue, newValue) => await button.InitializeMenu());

        public MapBase Map
        {
            get => (MapBase)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

#if AVALONIA
        [Content]
#endif
        public Collection<MapLayerItem> MapLayers { get; } = new ObservableCollection<MapLayerItem>();

        public Collection<MapLayerItem> MapOverlays { get; } = new ObservableCollection<MapLayerItem>();

        private async Task InitializeMenu()
        {
            if (Map != null)
            {
                var menu = CreateMenu();

                foreach (var item in MapLayers)
                {
                    menu.Items.Add(CreateMenuItem(item.Text, item, MapLayerClicked));
                }

                var initialLayer = MapLayers.Select(l => l.GetLayer()).FirstOrDefault();

                if (MapOverlays.Count > 0)
                {
                    if (initialLayer != null)
                    {
                        menu.Items.Add(CreateSeparator());
                    }

                    foreach (var item in MapOverlays)
                    {
                        menu.Items.Add(CreateMenuItem(item.Text, item, MapOverlayClicked));
                    }
                }

                if (initialLayer != null)
                {
                    SetMapLayer(await initialLayer);
                }
            }
        }

        private async void MapLayerClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var mapLayerItem = (MapLayerItem)item.Tag;

            SetMapLayer(await mapLayerItem.GetLayer());
        }

        private async void MapOverlayClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var mapLayerItem = (MapLayerItem)item.Tag;

            ToggleMapOverlay(await mapLayerItem.GetLayer());
        }

        private void SetMapLayer(FrameworkElement layer)
        {
            if (selectedLayer != layer)
            {
                selectedLayer = layer;
                Map.MapLayer = selectedLayer;
            }

            UpdateCheckedStates();
        }

        private void ToggleMapOverlay(FrameworkElement layer)
        {
            if (Map.Children.Contains(layer))
            {
                Map.Children.Remove(layer);
            }
            else
            {
                int index = 1;

                foreach (var overlay in MapOverlays.Select(o => o.Layer).Where(o => o != null))
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
                item.IsChecked = Map.Children.Contains(((MapLayerItem)item.Tag).Layer);
            }
        }
    }
}
