// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;

namespace MapControl
{
    /// <summary>
    /// Base class for child elements of a MapPanel.
    /// </summary>
    public abstract class MapElement : FrameworkElement
    {
        static MapElement()
        {
            MapPanel.ParentMapPropertyKey.OverrideMetadata(
                typeof(MapElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));
        }

        public MapBase ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
        }

        protected abstract void OnViewportChanged();

        private void OnViewportChanged(object sender, EventArgs e)
        {
            OnViewportChanged();
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MapElement mapElement = obj as MapElement;

            if (mapElement != null)
            {
                MapBase oldParentMap = e.OldValue as MapBase;
                MapBase newParentMap = e.NewValue as MapBase;

                if (oldParentMap != null)
                {
                    oldParentMap.ViewportChanged -= mapElement.OnViewportChanged;
                }

                if (newParentMap != null)
                {
                    newParentMap.ViewportChanged += mapElement.OnViewportChanged;
                }
            }
        }
    }
}
