// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MapControl
{
    public partial class MapImageLayer
    {
        public static void FadeOver(Image topImage, Image bottomImage)
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
                Duration = TimeSpan.Zero,
                FillBehavior = FillBehavior.Stop
            };

            topImage.BeginAnimation(OpacityProperty, fadeInAnimation);
            bottomImage.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }
}
