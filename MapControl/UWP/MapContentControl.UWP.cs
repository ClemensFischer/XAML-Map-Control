// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl
    {
        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.Register(
            nameof(AutoCollapse), typeof(bool), typeof(MapContentControl),
            new PropertyMetadata(false, (o, e) => MapPanel.SetAutoCollapse((MapContentControl)o, (bool)e.NewValue)));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapContentControl),
            new PropertyMetadata(null, (o, e) => MapPanel.SetLocation((MapContentControl)o, (Location)e.NewValue)));

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
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null)
            {
                this.ValidateProperty(BackgroundProperty, parentMap, nameof(MapBase.Background));
                this.ValidateProperty(BorderBrushProperty, parentMap, nameof(MapBase.Foreground));
                this.ValidateProperty(ForegroundProperty, parentMap, nameof(MapBase.Foreground));
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
