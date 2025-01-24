// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;

namespace MapControl
{
    public partial class MapImageLayer
    {
        public static void FadeOver(Image topImage, Image bottomImage)
        {
            var fadeInAnimation = new Animation
            {
                FillMode = FillMode.Forward,
                Duration = MapBase.ImageFadeDuration,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = MapBase.ImageFadeDuration,
                        Setters = { new Setter(OpacityProperty, 1d) }
                    }
                }
            };

            _ = fadeInAnimation.RunAsync(topImage).ContinueWith(
                _ => bottomImage.Opacity = 0d,
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
