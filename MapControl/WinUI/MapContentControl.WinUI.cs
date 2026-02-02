#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

namespace MapControl
{
    public partial class MapContentControl
    {
        public MapContentControl()
        {
            DefaultStyleKey = typeof(MapContentControl);
            MapPanel.InitMapElement(this);
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

    public partial class Pushpin
    {
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);
        }
    }
}
