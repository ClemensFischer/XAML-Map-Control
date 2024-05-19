// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Threading;
using System;

namespace MapControl
{
    internal static class Timer
    {
        public static DispatcherTimer CreateTimer(this AvaloniaObject obj, TimeSpan interval)
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
