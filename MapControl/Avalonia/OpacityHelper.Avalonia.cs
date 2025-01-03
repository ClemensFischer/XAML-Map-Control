﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;
using System;

namespace MapControl
{
    public static class OpacityHelper
    {
        public static Task FadeIn(Control element)
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

            return animation.RunAsync(element);
        }

        public static async Task SwapOpacitiesAsync(Control topElement, Control bottomElement)
        {
            var animation = new Animation
            {
                FillMode = FillMode.Forward,
                Duration = MapBase.ImageFadeDuration,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = MapBase.ImageFadeDuration,
                        Setters = { new Setter(Visual.OpacityProperty, 1d) }
                    }
                }
            };

            await animation.RunAsync(topElement);

            bottomElement.Opacity = 0d;
        }
    }
}
