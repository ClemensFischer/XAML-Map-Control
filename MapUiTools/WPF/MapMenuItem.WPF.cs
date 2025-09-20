using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public partial class MapMenuItem : MenuItem
    {
        protected MapMenuItem()
        {
            Loaded += (s, e) => Initialize();
            Click += (s, e) => Execute();
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();
    }
}
