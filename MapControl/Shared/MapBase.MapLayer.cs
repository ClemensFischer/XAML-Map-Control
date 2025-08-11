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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public static readonly DependencyProperty MapLayerProperty =
            DependencyPropertyHelper.Register<MapBase, FrameworkElement>(nameof(MapLayer), null,
                (map, oldValue, newValue) => map.MapLayerPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty MapLayerItemsSourceProperty =
            DependencyPropertyHelper.Register<MapBase, IEnumerable>(nameof(MapLayerItemsSource), null,
                (map, oldValue, newValue) => map.MapLayerItemsSourcePropertyChanged(oldValue, newValue));

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
        public FrameworkElement MapLayer
        {
            get => (FrameworkElement)GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
        }

        public IEnumerable MapLayerItemsSource
        {
            get => (IEnumerable)GetValue(MapLayerItemsSourceProperty);
            set => SetValue(MapLayerItemsSourceProperty, value);
        }

        private void MapLayerPropertyChanged(FrameworkElement oldLayer, FrameworkElement newLayer)
        {
            if (oldLayer != null)
            {
                if (Children.Count > 0 && Children[0] == oldLayer)
                {
                    Children.RemoveAt(0);
                }

                if (oldLayer is IMapLayer mapLayer)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        ClearValue(BackgroundProperty);
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        ClearValue(ForegroundProperty);
                    }
                }
            }

            if (newLayer != null)
            {
                if (Children.Count == 0 || Children[0] != newLayer)
                {
                    Children.Insert(0, newLayer);
                }

                if (newLayer is IMapLayer mapLayer)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        Background = mapLayer.MapBackground;
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        Foreground = mapLayer.MapForeground;
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
            var mapLayers = items.Cast<object>().Select(CreateMapLayer).ToList();
#if WPF
            // Execute at DispatcherPriority.DataBind to ensure that all bindings are evaluated.
            Dispatcher.Invoke(() => AddMapLayers(mapLayers, index), DispatcherPriority.DataBind);
#else
            AddMapLayers(mapLayers, index);
#endif
        }

        private void AddMapLayers(IEnumerable<FrameworkElement> mapLayers, int index)
        {
            foreach (var mapLayer in mapLayers)
            {
                Children.Insert(index, mapLayer);

                if (index++ == 0)
                {
                    MapLayer = mapLayer;
                }
            }
        }

        private void RemoveMapLayers(IEnumerable items, int index)
        {
            Children.RemoveRange(index, items.Cast<object>().Count());

            if (index == 0)
            {
                MapLayer = null;
            }
        }

        private FrameworkElement CreateMapLayer(object item)
        {
            FrameworkElement mapLayer = null;

            if (item != null)
            {
                mapLayer = item as FrameworkElement ?? TryLoadDataTemplate(item);
            }

            return mapLayer ?? new MapControl.MapPanel();
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
            if (this.TryFindResource(item.GetType().FullName) is DataTemplate template)
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
    }

#if UWP || WINUI
    internal static class MapBaseExtensions
    {
        public static void RemoveRange(this UIElementCollection elements, int index, int count)
        {
            while (--count >= 0)
            {
                elements.RemoveAt(index);
            }
        }

        public static object TryFindResource(this FrameworkElement element, object key)
        {
            return element.Resources.ContainsKey(key)
                ? element.Resources[key]
                : element.Parent is FrameworkElement parent
                ? TryFindResource(parent, key)
                : null;
        }
    }
#endif
}
