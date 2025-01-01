// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;

namespace MapControl
{
    internal static class Timer
    {
        public static DispatcherQueueTimer CreateTimer(this DependencyObject obj, TimeSpan interval)
        {
            var timer = obj.DispatcherQueue.CreateTimer();
            timer.Interval = interval;
            return timer;
        }

        public static void Run(this DispatcherQueueTimer timer, bool restart = false)
        {
            if (restart)
            {
                timer.Stop();
            }

            if (!timer.IsRunning)
            {
                timer.Start();
            }
        }
    }
}
