using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public partial class MapMenuItem : MenuItem
    {
        protected MapMenuItem()
        {
            Loaded += (_, _) => Initialize();
            Click += (_, _) => Execute();
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();
    }
}
