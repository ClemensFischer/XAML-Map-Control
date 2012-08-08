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
            MapPanel.ParentMapProperty.OverrideMetadata(typeof(MapElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));
        }

        protected MapElement()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        public Map ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
        }

        protected abstract void OnViewportChanged();

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            MapElement mapElement = obj as MapElement;

            if (mapElement != null)
            {
                Map oldParentMap = eventArgs.OldValue as Map;
                Map newParentMap = eventArgs.NewValue as Map;

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
