#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public partial class MapItem : ListBoxItem, IMapElement
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.AddOwner<MapItem, Location>(
                nameof(Location), MapPanel.LocationProperty, (item, _, _) => item.UpdateMapTransform());

        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapItem, bool>(
                nameof(AutoCollapse), MapPanel.AutoCollapseProperty);

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => (bool)GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get;
            set
            {
                if (field != null)
                {
                    field.ViewportChanged -= OnViewportChanged;
                }

                field = value;

                if (field != null && mapTransform != null)
                {
                    // Attach ViewportChanged handler only if MapTransform is actually used.
                    //
                    field.ViewportChanged += OnViewportChanged;
                    UpdateMapTransform();
                }
            }
        }

        /// <summary>
        /// Gets a Transform for scaling and rotating geometries
        /// in map coordinates (meters) to view coordinates (pixels).
        /// </summary>
        public MatrixTransform MapTransform
        {
            get
            {
                if (mapTransform == null)
                {
                    mapTransform = new MatrixTransform();

                    if (ParentMap != null)
                    {
                        ParentMap.ViewportChanged += OnViewportChanged;

                        UpdateMapTransform();
                    }
                }

                return mapTransform;
            }
        }

        private MatrixTransform mapTransform;

        private void UpdateMapTransform()
        {
            if (mapTransform != null && ParentMap != null && Location != null)
            {
                mapTransform.Matrix = ParentMap.GetMapToViewTransform(Location);
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateMapTransform();
        }
    }
}
