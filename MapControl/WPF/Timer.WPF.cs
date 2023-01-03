// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if UWP
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Threading;
#endif

namespace MapControl
{
    internal static class Timer
    {
        public static DispatcherTimer CreateTimer(this DependencyObject obj, TimeSpan interval)
        {
            var timer = new DispatcherTimer
            {
                Interval = interval
            };

            return timer;
        }

        public static void Run(this DispatcherTimer timer, bool restart = false)
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
