// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public partial class MapOverlaysPanel : MapPanel
    {
        public static readonly DependencyProperty SourcePathsProperty =
            DependencyPropertyHelper.Register<MapOverlaysPanel, IEnumerable<string>>(nameof(SourcePaths), null,
                (control, oldValue, newValue) => control.SourcePathsPropertyChanged(oldValue, newValue));

        public IEnumerable<string> SourcePaths
        {
            get => (IEnumerable<string>)GetValue(SourcePathsProperty);
            set => SetValue(SourcePathsProperty, value);
        }

        private void SourcePathsPropertyChanged(IEnumerable<string> oldSourcePaths, IEnumerable<string> newSourcePaths)
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

                AddOverlays(0, newSourcePaths);
            }
        }

        private void SourcePathsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddOverlays(args.NewStartingIndex, args.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveOverlays(args.OldStartingIndex, args.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Move:
                    RemoveOverlays(args.OldStartingIndex, args.OldItems.Count);
                    AddOverlays(args.NewStartingIndex, args.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    ReplaceOverlays(args.NewStartingIndex, args.NewItems.Cast<string>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Children.Clear();
                    AddOverlays(0, SourcePaths);
                    break;
            }
        }

        private void AddOverlays(int index, IEnumerable<string> sourcePaths)
        {
            foreach (var sourcePath in sourcePaths)
            {
                Children.Insert(index++, CreateOverlay(sourcePath));
            }
        }

        private void ReplaceOverlays(int index, IEnumerable<string> sourcePaths)
        {
            foreach (var sourcePath in sourcePaths)
            {
                Children[index++] = CreateOverlay(sourcePath);
            }
        }

        private void RemoveOverlays(int index, int count)
        {
            while (--count >= 0)
            {
                Children.RemoveAt(index);
            }
        }

        protected virtual FrameworkElement CreateOverlay(string sourcePath)
        {
            var ext = Path.GetExtension(sourcePath).ToLower();
            FrameworkElement overlay;

            try
            {
                if (ext == ".kmz" || ext == ".kml")
                {
                    overlay = new GroundOverlay { SourcePath = sourcePath };
                }
                else
                {
                    overlay = new GeoImage { SourcePath = sourcePath };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MapOverlaysControl: {sourcePath}: {ex.Message}");
                overlay = new MapPanel();
            }

            return overlay;
        }
    }
}
