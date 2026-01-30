#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public partial class MapContentControl : ContentControl
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.Register<MapContentControl, bool>(nameof(AutoCollapse), false,
                (control, oldValue, newValue) => MapPanel.SetAutoCollapse(control, newValue));

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapContentControl, Location>(nameof(Location), null,
                (control, oldValue, newValue) => MapPanel.SetLocation(control, newValue));

        public MapContentControl()
        {
            DefaultStyleKey = typeof(MapContentControl);
            MapPanel.InitMapElement(this);
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
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null)
            {
                // Workaround for missing RelativeSource AncestorType=MapBase Bindings in default Style.
                //
                if (Background == null)
                {
                    SetBinding(BackgroundProperty,
                        new Binding { Source = parentMap, Path = new PropertyPath(nameof(Background)) });
                }
                if (Foreground == null)
                {
                    SetBinding(ForegroundProperty,
                        new Binding { Source = parentMap, Path = new PropertyPath(nameof(Foreground)) });
                }
                if (BorderBrush == null)
                {
                    SetBinding(BorderBrushProperty,
                        new Binding { Source = parentMap, Path = new PropertyPath(nameof(Foreground)) });
                }
            }
        }
    }

    /// <summary>
    /// MapContentControl with a Pushpin Style.
    /// </summary>
    public class Pushpin : MapContentControl
    {
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);
        }
    }
}
