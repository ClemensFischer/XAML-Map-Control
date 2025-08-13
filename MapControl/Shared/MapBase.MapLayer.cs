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
        public static readonly DependencyProperty MapLayerProperty =
            DependencyPropertyHelper.Register<MapBase, object>(nameof(MapLayer), null,
                (map, oldValue, newValue) => map.MapLayerPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty MapLayersSourceProperty =
            DependencyPropertyHelper.Register<MapBase, IEnumerable>(nameof(MapLayersSource), null,
                (map, oldValue, newValue) => map.MapLayersSourcePropertyChanged(oldValue, newValue));

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the passed object is not a FrameworkElement, MapBase tries to locate a DataTemplate
        /// resource for the object's type and generate a FrameworkElement from that DataTemplate.
        /// If the FrameworkElement implements IMapLayer (like e.g. MapTileLayer or MapImageLayer),
        /// its (non-null) MapBackground and MapForeground property values are used for the MapBase
        /// Background and Foreground.
        /// </summary>
        public object MapLayer
        {
            get => GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
        }

        /// <summary>
        /// Holds a collection of map layers, either FrameworkElements or plain objects with
        /// an associated DataTemplate resource from which a FrameworkElement can be created.
        /// FrameworkElemens are added to the Children collection, starting at index 0.
        /// The first element of this collection is assigned to the MapLayer property.
        /// Subsequent changes of the MapLayer or Children properties are not reflected
        /// by the MapLayersSource collection.
        /// </summary>
        public IEnumerable MapLayersSource
        {
            get => (IEnumerable)GetValue(MapLayersSourceProperty);
            set => SetValue(MapLayersSourceProperty, value);
        }

        private void MapLayerPropertyChanged(object oldLayer, object newLayer)
        {
            bool IsMapLayer(object layer) => Children.Count > 0 &&
                (Children[0] == layer as FrameworkElement ||
                ((FrameworkElement)Children[0]).DataContext == layer);

            if (oldLayer != null && IsMapLayer(oldLayer))
            {
                RemoveChildElement(0);
            }

            if (newLayer != null && !IsMapLayer(newLayer))
            {
                InsertChildElement(0, GetMapLayer(newLayer));
            }
        }

        private void MapLayersSourcePropertyChanged(IEnumerable oldLayers, IEnumerable newLayers)
        {
            if (oldLayers != null)
            {
                if (oldLayers is INotifyCollectionChanged incc)
                {
                    incc.CollectionChanged -= MapLayersSourceCollectionChanged;
                }

                RemoveMapLayers(oldLayers, 0);
            }

            if (newLayers != null)
            {
                if (newLayers is INotifyCollectionChanged incc)
                {
                    incc.CollectionChanged += MapLayersSourceCollectionChanged;
                }

                AddMapLayers(newLayers, 0);
            }
        }

        private void MapLayersSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        private void AddMapLayers(IEnumerable layers, int index)
        {
            var mapLayers = layers.Cast<object>().Select(GetMapLayer).ToList();

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
                InsertChildElement(index, mapLayer);

                if (index++ == 0)
                {
                    MapLayer = mapLayer;
                }
            }
        }

        private void RemoveMapLayers(IEnumerable layers, int index)
        {
            foreach (var _ in layers)
            {
                RemoveChildElement(index);
            }

            if (index == 0)
            {
                MapLayer = null;
            }
        }

        private void InsertChildElement(int index, FrameworkElement element)
        {
            if (index == 0 && element is IMapLayer mapLayer)
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

            Children.Insert(index, element);
        }

        private void RemoveChildElement(int index)
        {
            if (index == 0 && Children[0] is IMapLayer mapLayer)
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

            Children.RemoveAt(index);
        }

        private FrameworkElement GetMapLayer(object layer)
        {
            FrameworkElement mapLayer = null;

            if (layer != null)
            {
                mapLayer = layer as FrameworkElement ?? TryLoadDataTemplate(layer);
            }

            return mapLayer ?? new MapTileLayer();
        }

        private FrameworkElement TryLoadDataTemplate(object layer)
        {
            FrameworkElement element = null;
#if AVALONIA
            if (this.TryFindResource(layer.GetType().FullName, out object value) &&
                value is Avalonia.Markup.Xaml.Templates.DataTemplate template)
            {
                element = template.Build(layer);
            }
#elif WPF
            if (TryFindResource(new DataTemplateKey(layer.GetType())) is DataTemplate template)
            {
                element = (FrameworkElement)template.LoadContent();
            }
#else
            if (TryFindResource(this, layer.GetType().FullName) is DataTemplate template)
            {
                element = (FrameworkElement)template.LoadContent();
            }
#endif
            if (element != null)
            {
                element.DataContext = layer;
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
