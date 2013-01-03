// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)


#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public readonly Image Image = new Image { Stretch = Stretch.Uniform, Opacity = 0d };

        public ImageSource ImageSource
        {
            get { return Image.Source; }
        }

        public void SetImageSource(ImageSource image, bool animateOpacity)
        {
            if (Image.Source == null)
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
                        Image.BeginAnimation(Image.OpacityProperty, OpacityAnimation);
                    }
                }
                else
                {
                    Image.Opacity = 1d;
                }
            }

            Image.Source = image;
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            Image.BeginAnimation(Image.OpacityProperty, OpacityAnimation);
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            Image.Source = null;
        }
    }
}
