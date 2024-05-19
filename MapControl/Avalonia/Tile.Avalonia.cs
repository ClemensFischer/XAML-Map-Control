// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Animation;
using Avalonia.Styling;
using System;

namespace MapControl
{
    public partial class Tile
    {
        private void AnimateImageOpacity()
        {
            var animation = new Animation
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

            _ = animation.RunAsync(Image);
        }
    }
}
