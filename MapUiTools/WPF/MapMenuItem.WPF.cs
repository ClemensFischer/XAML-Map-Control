using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public abstract partial class MapMenuItem : MenuItem
    {
        public abstract bool GetIsChecked(MapBase map);

        public abstract Task ExecuteAsync(MapBase map);

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
                    await ExecuteAsync(map);

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
    }
}
