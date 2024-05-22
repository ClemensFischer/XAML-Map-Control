// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace MapControl
{
    public static class OpacityHelper
    {
        public static void BeginOpacityAnimation(DependencyObject obj, DoubleAnimation animation)
        {
            Storyboard.SetTargetProperty(animation, nameof(UIElement.Opacity));
            Storyboard.SetTarget(animation, obj);

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        public static async Task SwapOpacities(UIElement topElement, UIElement bottomElement)
        {
            BeginOpacityAnimation(topElement, new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            });

            BeginOpacityAnimation(bottomElement, new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            });

            await Task.CompletedTask;
        }
    }
}
