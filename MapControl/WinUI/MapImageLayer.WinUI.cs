// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace MapControl
{
    public partial class MapImageLayer
    {
        public static void FadeOver(Image topImage, Image bottomImage)
        {
            var topImageAnimation = new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            };

            var bottomImageAnimation = new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            };

            Storyboard.SetTargetProperty(topImageAnimation, nameof(Opacity));
            Storyboard.SetTarget(topImageAnimation, topImage);

            Storyboard.SetTargetProperty(bottomImageAnimation, nameof(Opacity));
            Storyboard.SetTarget(bottomImageAnimation, bottomImage);

            var storyboard = new Storyboard();
            storyboard.Children.Add(topImageAnimation);
            storyboard.Children.Add(bottomImageAnimation);
            storyboard.Begin();
        }
    }
}
