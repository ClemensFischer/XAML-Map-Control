﻿#if WPF
using System.Windows.Controls;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

using System;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public partial class MapItem : ListBoxItem, IMapElement
    {
        private MapBase parentMap;
        private MatrixTransform mapTransform;

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => (bool)GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get => parentMap;
            set
            {
                if (parentMap != null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                parentMap = value;

                if (parentMap != null && mapTransform != null)
                {
                    // Attach ViewportChanged handler only if MapTransform is actually used.
                    //
                    parentMap.ViewportChanged += OnViewportChanged;

                    UpdateMapTransform();
                }
            }
        }

        /// <summary>
        /// Gets a Transform for scaling and rotating geometries
        /// in map coordinates (meters) to view coordinates (pixels).
        /// </summary>
        public Transform MapTransform
        {
            get
            {
                if (mapTransform == null)
                {
                    mapTransform = new MatrixTransform();

                    if (parentMap != null)
                    {
                        parentMap.ViewportChanged += OnViewportChanged;

                        UpdateMapTransform();
                    }
                }

                return mapTransform;
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateMapTransform();
        }

        private void UpdateMapTransform()
        {
            if (mapTransform != null && parentMap != null && Location != null)
            {
                mapTransform.Matrix = parentMap.GetMapTransform(Location);
            }
        }
    }
}
