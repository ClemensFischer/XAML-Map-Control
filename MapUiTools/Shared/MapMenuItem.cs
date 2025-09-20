using System.Threading.Tasks;

namespace MapControl.UiTools
{
    public abstract partial class MapMenuItem
    {
        public abstract Task ExecuteAsync(MapBase map);

        protected abstract bool GetIsChecked(MapBase map);

        protected virtual bool GetIsEnabled(MapBase map) => true;

        private void Initialize()
        {
            if (DataContext is MapBase map)
            {
                IsEnabled = GetIsEnabled(map);
                IsChecked = GetIsChecked(map);
            }
        }

        private async void Execute()
        {
            if (DataContext is MapBase map)
            {
                await ExecuteAsync(map);
            }
        }
    }
}
