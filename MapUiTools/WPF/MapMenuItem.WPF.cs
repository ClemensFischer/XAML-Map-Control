using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public abstract class MapMenuItem : MenuItem
    {
        protected MapMenuItem()
        {
            Loaded += (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    IsChecked = GetIsChecked(map);
                }
            };

            Click += async (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    await Execute(map);

                    foreach (var item in ParentMenuItems)
                    {
                        item.IsChecked = item.GetIsChecked(map);
                    }
                }
            };
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();

        protected abstract bool GetIsChecked(MapBase map);

        public abstract Task Execute(MapBase map);
    }
}
