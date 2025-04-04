﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#endif

namespace MapControl
{
    /// <summary>
    /// A MapPanel with a collection of GroundOverlay or GeoImage children.
    /// </summary>
    public class MapOverlaysPanel : MapPanel
    {
        public static readonly DependencyProperty SourcePathsProperty =
            DependencyPropertyHelper.Register<MapOverlaysPanel, IEnumerable<string>>(nameof(SourcePaths), null,
                async (control, oldValue, newValue) => await control.SourcePathsPropertyChanged(oldValue, newValue));

        public IEnumerable<string> SourcePaths
        {
            get => (IEnumerable<string>)GetValue(SourcePathsProperty);
            set => SetValue(SourcePathsProperty, value);
        }

        private async Task SourcePathsPropertyChanged(IEnumerable<string> oldSourcePaths, IEnumerable<string> newSourcePaths)
        {
            Children.Clear();

            if (oldSourcePaths is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= SourcePathsCollectionChanged;
            }

            if (newSourcePaths != null)
            {
                if (newSourcePaths is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += SourcePathsCollectionChanged;
                }

                await AddOverlays(0, newSourcePaths);
            }
        }

        private async void SourcePathsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    await AddOverlays(e.NewStartingIndex, e.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveOverlays(e.OldStartingIndex, e.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Move:
                    RemoveOverlays(e.OldStartingIndex, e.OldItems.Count);
                    await AddOverlays(e.NewStartingIndex, e.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    await ReplaceOverlays(e.NewStartingIndex, e.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Children.Clear();
                    await AddOverlays(0, SourcePaths);
                    break;
            }
        }

        private async Task AddOverlays(int index, IEnumerable<string> sourcePaths)
        {
            foreach (var sourcePath in sourcePaths)
            {
                Children.Insert(index++, await CreateOverlayAsync(sourcePath));
            }
        }

        private async Task ReplaceOverlays(int index, IEnumerable<string> sourcePaths)
        {
            foreach (var sourcePath in sourcePaths)
            {
                Children[index++] = await CreateOverlayAsync(sourcePath);
            }
        }

        private void RemoveOverlays(int index, int count)
        {
            while (--count >= 0)
            {
                Children.RemoveAt(index);
            }
        }

        protected virtual async Task<FrameworkElement> CreateOverlayAsync(string sourcePath)
        {
            FrameworkElement overlay;
            var ext = Path.GetExtension(sourcePath).ToLower();

            if (ext == ".kmz" || ext == ".kml")
            {
                overlay = await GroundOverlay.CreateAsync(sourcePath);
            }
            else
            {
                overlay = await GeoImage.CreateAsync(sourcePath);
            }

            return overlay;
        }
    }
}
