// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() =>
            {
                using (var stream = new MemoryStream(buffer))
                {
                    return LoadImage(stream);
                }
            });
        }

        public static Task<ImageSource> LoadImageAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                using (var stream = File.OpenRead(path))
                {
                    return LoadImage(stream);
                }
            });
        }

        private static ImageSource LoadImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
