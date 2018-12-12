// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using MapControl.Projections;
#if WINDOWS_UWP
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

namespace MapControl.Images
{
    public static class WorldFile
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.RegisterAttached(
            "ImagePath", typeof(string), typeof(WorldFile),
            new PropertyMetadata(null, async (o, e) => await SetWorldImageAsync((Image)o, (string)e.NewValue)));

        public static string GetImagePath(this Image image)
        {
            return (string)image.GetValue(ImagePathProperty);
        }

        public static void SetImagePath(this Image image, string imagePath)
        {
            image.SetValue(ImagePathProperty, imagePath);
        }

        public static void SetWorldImage(this Image image, BitmapSource bitmapSource, WorldFileParameters parameters, MapProjection projection = null)
        {
            image.Source = bitmapSource;
            image.Stretch = Stretch.Fill;

            MapPanel.SetBoundingBox(image, parameters.GetBoundingBox(bitmapSource.PixelWidth, bitmapSource.PixelHeight, projection));
        }

        private static async Task SetWorldImageAsync(Image image, string imagePath)
        {
            var ext = Path.GetExtension(imagePath);
            if (ext.Length < 4)
            {
                throw new ArgumentException("Invalid image file path extension, must have at least three characters.");
            }

            BitmapSource bitmap;
            using (var stream = File.OpenRead(imagePath))
            {
#if WINDOWS_UWP
                bitmap = await ImageLoader.LoadImageAsync(stream.AsRandomAccessStream());
#else
                bitmap = await ImageLoader.LoadImageAsync(stream);
#endif
            }

            var dir = Path.GetDirectoryName(imagePath);
            var file = Path.GetFileNameWithoutExtension(imagePath);
            var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");
            var projFilePath = Path.Combine(dir, file + ".prj");

            var parameters = new WorldFileParameters(worldFilePath);
            MapProjection projection = null;

            if (File.Exists(projFilePath))
            {
                projection = new GeoApiProjection { WKT = File.ReadAllText(projFilePath) };
            }

            SetWorldImage(image, bitmap, parameters, projection);
        }
    }
}
