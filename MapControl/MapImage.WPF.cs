// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class MapImage
    {
        private static readonly Transform imageTransform = new ScaleTransform(1d, -1d);

        private void SourceChanged(ImageSource image)
        {
            var bitmap = image as BitmapSource;

            if (bitmap != null)
            {
                image = new TransformedBitmap(bitmap, imageTransform);
            }

            Fill = new ImageBrush { ImageSource = image };
            SetBrushTransform();
        }
    }
}