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
            topImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            });

            bottomImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            });
        }
    }
}
