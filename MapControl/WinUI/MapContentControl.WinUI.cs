// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.Register<MapContentControl, bool>(nameof(AutoCollapse), false, false,
                (control, oldValue, newValue) => MapPanel.SetAutoCollapse(control, newValue));

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapContentControl, Location>(nameof(Location), null, false,
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
                // If this.Background is not explicitly set, bind it to parentMap.Background.
                //
                this.SetBindingOnUnsetProperty(BackgroundProperty, parentMap, Panel.BackgroundProperty, nameof(Background));

                // If this.Foreground is not explicitly set, bind it to parentMap.Foreground.
                //
                this.SetBindingOnUnsetProperty(ForegroundProperty, parentMap, MapBase.ForegroundProperty, nameof(Foreground));

                // If this.BorderBrush is not explicitly set, bind it to parentMap.Foreground.
                //
                this.SetBindingOnUnsetProperty(BorderBrushProperty, parentMap, MapBase.ForegroundProperty, nameof(Foreground));
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
