#if WPF
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using Avalonia.Threading;
#endif

namespace MapControl;

internal static class DispatcherTimerExtension
{
    extension(DispatcherTimer timer)
    {
        public void Run(bool restart = false)
        {
            if (restart)
            {
                timer.Stop();
            }

            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }
    }
}
