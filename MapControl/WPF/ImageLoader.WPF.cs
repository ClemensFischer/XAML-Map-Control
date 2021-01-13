// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static ImageSource LoadImage(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return LoadImage(stream);
            }
        }

        public static ImageSource LoadImage(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    image = LoadImage(stream);
                }
            }

            return image;
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() => LoadImage(buffer));
        }

        public static Task<ImageSource> LoadImageAsync(string path)
        {
            return Task.Run(() => LoadImage(path));
        }
    }
}
