// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public partial class Tile
    {
        private void FadeIn()
        {
            var fadeInAnimation = new Animation
            {
                Duration = MapBase.ImageFadeDuration,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters = { new Setter(Visual.OpacityProperty, 0d) }
                    },
                    new KeyFrame
                    {
                        KeyTime = MapBase.ImageFadeDuration,
                        Setters = { new Setter(Visual.OpacityProperty, 1d) }
                    }
                }
            };

            _ = fadeInAnimation.RunAsync(Image);
        }
    }
}
