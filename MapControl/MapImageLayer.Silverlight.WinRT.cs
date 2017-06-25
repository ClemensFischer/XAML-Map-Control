﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected void UpdateImage(Uri uri)
        {
            UpdateImage(new BitmapImage(uri));
        }

        protected void UpdateImage(BitmapSource bitmap)
        {
            SetTopImage(bitmap);

            var bitmapImage = bitmap as BitmapImage;

            if (bitmapImage != null)
            {
                bitmapImage.ImageOpened += BitmapImageOpened;
                bitmapImage.ImageFailed += BitmapImageFailed;
            }
            else
            {
                SwapImages();
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;
            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            SwapImages();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;
            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            ((Image)Children[topImageIndex]).Source = null;
            SwapImages();
        }
    }
}
