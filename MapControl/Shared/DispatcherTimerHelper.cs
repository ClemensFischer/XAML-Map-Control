#if WPF
using System.Windows;
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
global using DispatcherTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;
using Microsoft.UI.Xaml;
#elif AVALONIA
using DependencyObject = Avalonia.AvaloniaObject;
using Avalonia.Threading;
#endif
using System;

namespace MapControl
{
    internal static class DispatcherTimerHelper
    {
        public static DispatcherTimer CreateTimer(this DependencyObject obj, TimeSpan interval)
        {
#if WINUI
            var timer = obj.DispatcherQueue.CreateTimer();
#else
            var timer = new DispatcherTimer();
#endif
            timer.Interval = interval;
            return timer;
        }

        public static void Run(this DispatcherTimer timer, bool restart = false)
        {
            if (restart)
            {
                timer.Stop();
            }
#if WINUI
            if (!timer.IsRunning)
#else
            if (!timer.IsEnabled)
#endif
            {
                timer.Start();
            }
        }
    }
}
