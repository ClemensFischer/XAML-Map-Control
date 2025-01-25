// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public partial class Tile
    {
        private void BeginFadeInAnimation()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(fadeInAnimation, Image);
            Storyboard.SetTargetProperty(fadeInAnimation, nameof(UIElement.Opacity));

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin();
        }

        private void FadeIn()
        {
            if (Image.Source is BitmapImage bitmap && bitmap.UriSource != null)
            {
                bitmap.ImageOpened += BitmapImageOpened;
                bitmap.ImageFailed += BitmapImageFailed;
            }
            else
            {
                BeginFadeInAnimation();
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            BeginFadeInAnimation();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            Image.Source = null;
        }
    }
}
