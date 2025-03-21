using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public abstract class MapMenuItem : MenuItem
    {
        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        public abstract Task Execute(MapBase map);

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();
    }
}
