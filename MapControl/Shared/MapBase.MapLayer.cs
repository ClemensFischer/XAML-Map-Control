using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public interface IMapLayer : IMapElement
    {
        Brush MapBackground { get; }
        Brush MapForeground { get; }
    }

    public partial class MapBase
    {
        private bool hasMapLayerBackground;
        private bool hasMapLayerForeground;

        public static readonly DependencyProperty MapLayerProperty =
            DependencyPropertyHelper.Register<MapBase, object>(nameof(MapLayer), null,
                (map, oldValue, newValue) => map.MapLayerPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty MapLayerItemsSourceProperty =
            DependencyPropertyHelper.Register<MapBase, IEnumerable>(nameof(MapLayerItemsSource), null,
                (map, oldValue, newValue) => map.MapLayerItemsSourcePropertyChanged(oldValue, newValue));

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
        public object MapLayer
        {
            get => GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
        }

        public IEnumerable MapLayerItemsSource
        {
            get => (IEnumerable)GetValue(MapLayerItemsSourceProperty);
            set => SetValue(MapLayerItemsSourceProperty, value);
        }

        private void MapLayerPropertyChanged(object oldLayer, object newLayer)
        {
            var firstChild = Children.Cast<FrameworkElement>().FirstOrDefault();

            if (oldLayer != null)
            {
                if (firstChild != null &&
                    (firstChild == oldLayer as FrameworkElement || firstChild.DataContext == oldLayer))
                {
                    Children.RemoveAt(0);
                }

                if (hasMapLayerBackground)
                {
                    ClearValue(BackgroundProperty);
                }

                if (hasMapLayerForeground)
                {
                    ClearValue(ForegroundProperty);
                }
            }

            hasMapLayerBackground = false;
            hasMapLayerForeground = false;

            if (newLayer != null)
            {
                if (firstChild == null ||
                    firstChild != newLayer as FrameworkElement && firstChild.DataContext != newLayer)
                {
                    Children.Insert(0, GetMapLayer(newLayer));
                }

                if (Children.Cast<FrameworkElement>().FirstOrDefault() is IMapLayer mapLayer)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        Background = mapLayer.MapBackground;
                        hasMapLayerBackground = true;
                    }

                    if (mapLayer.MapForeground != null)
                    {
                        Foreground = mapLayer.MapForeground;
                        hasMapLayerForeground = true;
                    }
                }
            }
        }

        private void MapLayerItemsSourcePropertyChanged(IEnumerable oldItems, IEnumerable newItems)
        {
            if (oldItems != null)
            {
                if (oldItems is INotifyCollectionChanged incc)
                {
                    incc.CollectionChanged -= MapLayerItemsSourceCollectionChanged;
                }

                RemoveMapLayers(oldItems, 0);
            }

            if (newItems != null)
            {
                if (newItems is INotifyCollectionChanged incc)
                {
                    incc.CollectionChanged += MapLayerItemsSourceCollectionChanged;
                }

                AddMapLayers(newItems, 0);
            }
        }

        private void MapLayerItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddMapLayers(e.NewItems, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveMapLayers(e.OldItems, e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    RemoveMapLayers(e.OldItems, e.OldStartingIndex);
                    AddMapLayers(e.NewItems, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    break;
            }
        }

        private void AddMapLayers(IEnumerable items, int index)
        {
            var mapLayers = items.Cast<object>().Select(GetMapLayer).ToList();

            if (mapLayers.Count > 0)
            {
#if WPF
                // Execute at DispatcherPriority.DataBind to ensure that all bindings are evaluated.
                Dispatcher.Invoke(() => AddMapLayers(mapLayers, index), DispatcherPriority.DataBind);
#else
                AddMapLayers(mapLayers, index);
#endif
            }
        }

        private void AddMapLayers(List<FrameworkElement> mapLayers, int index)
        {
            foreach (var mapLayer in mapLayers)
            {
                Children.Insert(index, mapLayer);

                if (index == 0)
                {
                    MapLayer = mapLayer;
                }
            }
        }

        private void RemoveMapLayers(IEnumerable items, int index)
        {
            foreach (var _ in items)
            {
                Children.RemoveAt(index);
            }

            if (index == 0)
            {
                MapLayer = null;
            }
        }

        private FrameworkElement GetMapLayer(object item)
        {
            FrameworkElement mapLayer = null;

            if (item != null)
            {
                mapLayer = item as FrameworkElement ?? TryLoadDataTemplate(item);
            }

            return mapLayer ?? new MapTileLayer();
        }

        private FrameworkElement TryLoadDataTemplate(object item)
        {
            FrameworkElement element = null;
#if AVALONIA
            if (this.TryFindResource(item.GetType().FullName, out object value) &&
                value is Avalonia.Markup.Xaml.Templates.DataTemplate template)
            {
                element = template.Build(item);
            }
#elif WPF
            if (TryFindResource(new DataTemplateKey(item.GetType())) is DataTemplate template)
            {
                element = (FrameworkElement)template.LoadContent();
            }
#else
            if (TryFindResource(this, item.GetType().FullName) is DataTemplate template)
            {
                element = (FrameworkElement)template.LoadContent();
            }
#endif
            if (element != null)
            {
                element.DataContext = item;
            }

            return element;
        }

#if UWP || WINUI
            private static object TryFindResource(FrameworkElement element, object key)
            {
                return element.Resources.ContainsKey(key)
                    ? element.Resources[key]
                    : element.Parent is FrameworkElement parent
                    ? TryFindResource(parent, key)
                    : null;
            }
#endif
    }
}
