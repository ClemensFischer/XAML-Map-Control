// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#else
using System.Windows;
using System.Windows.Markup;
#endif

namespace MapControl.UiTools
{
#if WINUI || UWP
    [ContentProperty(Name = nameof(Layer))]
#else
    [ContentProperty(nameof(Layer))]
#endif
    public class MapLayerItem
    {
        public string Text { get; set; }
        public UIElement Layer { get; set; }
        public Func<UIElement> LayerFactory { get; set; }

        public UIElement GetLayer() => Layer ?? (Layer = LayerFactory?.Invoke());
    }

#if WINUI || UWP
    [ContentProperty(Name = nameof(MapLayers))]
#else
    [ContentProperty(nameof(MapLayers))]
#endif
    public class MapLayersMenuButton : MenuButton
    {
        private UIElement selectedLayer;

        public MapLayersMenuButton()
            : base("\uE81E")
        {
            ((INotifyCollectionChanged)MapLayers).CollectionChanged += (s, e) => InitializeMenu();
            ((INotifyCollectionChanged)MapOverlays).CollectionChanged += (s, e) => InitializeMenu();
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            nameof(Map), typeof(MapBase), typeof(MapLayersMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapLayersMenuButton)o).InitializeMenu()));

        public MapBase Map
        {
            get => (MapBase)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        public Collection<MapLayerItem> MapLayers { get; } = new ObservableCollection<MapLayerItem>();

        public Collection<MapLayerItem> MapOverlays { get; } = new ObservableCollection<MapLayerItem>();

        private void InitializeMenu()
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
                    SetMapLayer(initialLayer);
                }
            }
        }

        private void MapLayerClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var mapLayerItem = (MapLayerItem)item.Tag;

            SetMapLayer(mapLayerItem.GetLayer());
        }

        private void MapOverlayClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var mapLayerItem = (MapLayerItem)item.Tag;

            ToggleMapOverlay(mapLayerItem.GetLayer());
        }

        private void SetMapLayer(UIElement layer)
        {
            if (selectedLayer != layer)
            {
                selectedLayer = layer;
                Map.MapLayer = selectedLayer;
            }

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
