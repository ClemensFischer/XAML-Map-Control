// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)


#if WINRT
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
    internal partial class Tile
    {
        public readonly Image Image = new Image { Stretch = Stretch.Uniform, Opacity = 0d };

        public ImageSource ImageSource
        {
            get { return Image.Source; }
            private set { Image.Source = value; }
        }

        public void SetImageSource(ImageSource source, bool animateOpacity)
        {
            if (ImageSource == null)
            {
                if (animateOpacity)
                {
                    var bitmap = source as BitmapImage;

                    if (bitmap != null) // TODO Check if bitmap is downloading somehow, maybe PixelWidth == 0?
                    {
                        bitmap.ImageOpened += BitmapImageOpened;
                        bitmap.ImageFailed += BitmapImageFailed;
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

            ImageSource = source;
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
            ImageSource = null;
        }
    }
}
