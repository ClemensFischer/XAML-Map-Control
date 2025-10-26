#if WPF
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using Avalonia.Threading;
#endif

namespace MapControl
{
    internal class UpdateTimer : DispatcherTimer
    {
        public void Run(bool restart = false)
        {
            if (restart)
            {
                Stop();
            }

            if (!IsEnabled)
            {
                Start();
            }
        }
    }
}
