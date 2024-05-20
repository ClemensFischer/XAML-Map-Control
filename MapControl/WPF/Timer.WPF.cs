// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if AVALONIA
using Avalonia.Threading;
#elif UWP
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif

namespace MapControl
{
    internal static class Timer
    {
        public static DispatcherTimer CreateTimer(this object _, TimeSpan interval)
        {
            return new DispatcherTimer { Interval = interval };
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
