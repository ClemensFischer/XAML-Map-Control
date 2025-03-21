using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public class MapMenuItem : MenuItem
    {
        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems
            => (Parent as ItemsControl)?.Items.OfType<MapMenuItem>();
    }
}
