// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace MapControl
{
    public static class OpacityHelper
    {
        public static Task SwapOpacitiesAsync(UIElement topElement, UIElement bottomElement)
        {
            topElement.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            });

            bottomElement.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            });

            return Task.CompletedTask;
        }
    }
}
