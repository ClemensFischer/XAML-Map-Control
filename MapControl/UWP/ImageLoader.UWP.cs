// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#if WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream stream)
        {
            var image = new BitmapImage();

            await image.SetSourceAsync(stream);

            return image;
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return LoadImageAsync(stream.AsRandomAccessStream());
        }

        public static Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return LoadImageAsync(stream);
            }
        }

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path));

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }
    }
}
