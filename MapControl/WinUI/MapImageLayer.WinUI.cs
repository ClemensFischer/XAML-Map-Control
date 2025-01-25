// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if UWP
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace MapControl
{
    public partial class MapImageLayer
    {
        private void FadeOver()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            };

            var fadeOutAnimation = new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            };

            Storyboard.SetTarget(fadeInAnimation, Children[1]);
            Storyboard.SetTargetProperty(fadeInAnimation, nameof(Opacity));

            Storyboard.SetTarget(fadeOutAnimation, Children[0]);
            Storyboard.SetTargetProperty(fadeOutAnimation, nameof(Opacity));

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(fadeOutAnimation);
            storyboard.Begin();
        }
    }
}
