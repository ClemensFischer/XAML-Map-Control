// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    /// <summary>
    /// Stores global static properties that control the behaviour of the map control.
    /// </summary>
    public static class Settings
    {
        public static TimeSpan TileUpdateInterval { get; set; }
        public static TimeSpan TileAnimationDuration { get; set; }
        public static TimeSpan MapAnimationDuration { get; set; }
        public static EasingFunctionBase MapAnimationEasingFunction { get; set; }

        static Settings()
        {
            TileUpdateInterval = TimeSpan.FromSeconds(0.5);
            TileAnimationDuration = TimeSpan.FromSeconds(0.3);
            MapAnimationDuration = TimeSpan.FromSeconds(0.3);
            MapAnimationEasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        }
    }
}
