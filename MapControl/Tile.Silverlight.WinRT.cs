// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public void SetImage(ImageSource image, bool animateOpacity = true, bool isDownloading = true)
        {
            Pending = false;

            if (image != null)
            {
                if (animateOpacity && OpacityAnimationDuration > TimeSpan.Zero)
                {
                    BitmapImage bitmap;

                    if (isDownloading && (bitmap = image as BitmapImage) != null)
                    {
                        bitmap.ImageOpened += BitmapImageOpened;
                        bitmap.ImageFailed += BitmapImageFailed;
                    }
                    else
                    {
                        Image.BeginAnimation(Image.OpacityProperty,
                            new DoubleAnimation { From = 0d, To = 1d, Duration = OpacityAnimationDuration });
                    }
                }
                else
                {
                    Image.Opacity = 1d;
                }

                Image.Source = image;
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            Image.BeginAnimation(Image.OpacityProperty,
                new DoubleAnimation { From = 0d, To = 1d, Duration = OpacityAnimationDuration });
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
