// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

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
        public readonly Image Image = new Image { Opacity = 0d };

        public ImageSource ImageSource
        {
            get { return Image.Source; }
        }

        public void SetImageSource(ImageSource image, bool animateOpacity)
        {
            if (image != null && Image.Source == null)
            {
                if (animateOpacity)
                {
                    var bitmapImage = image as BitmapImage;

                    if (bitmapImage != null)
                    {
                        bitmapImage.ImageOpened += BitmapImageOpened;
                        bitmapImage.ImageFailed += BitmapImageFailed;
                    }
                    else
                    {
                        Image.BeginAnimation(Image.OpacityProperty, new DoubleAnimation { To = 1d, Duration = AnimationDuration });
                    }
                }
                else
                {
                    Image.Opacity = 1d;
                }
            }

            Image.Source = image;
            HasImage = true;
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            Image.BeginAnimation(Image.OpacityProperty, new DoubleAnimation { To = 1d, Duration = AnimationDuration });
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            Image.Source = null;
        }
    }
}
