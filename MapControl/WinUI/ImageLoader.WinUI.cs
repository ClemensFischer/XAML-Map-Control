using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Uri uri)
        {
            return new BitmapImage(uri);
        }

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

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource image = null;

            path = FilePath.GetFullPath(path);

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(path);

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }

        internal static async Task<WriteableBitmap> LoadWriteableBitmapAsync(BitmapDecoder decoder)
        {
            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

            pixelData.DetachPixelData().CopyTo(image.PixelBuffer);

            return image;
        }

        internal static async Task<WriteableBitmap> LoadWriteableBitmapAsync(Uri uri, IProgress<double> progress, CancellationToken cancellationToken)
        {
            WriteableBitmap bitmap = null;

            progress.Report(0d);

            try
            {
                var response = await GetHttpResponseAsync(uri, progress, cancellationToken);

                if (response?.Buffer != null)
                {
                    using (var memoryStream = new MemoryStream(response.Buffer))
                    using (var randomAccessStream = memoryStream.AsRandomAccessStream())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

                        bitmap = await LoadWriteableBitmapAsync(decoder);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "{uri}", uri);
            }

            progress.Report(1d);

            return bitmap;
        }

        internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress, CancellationToken cancellationToken)
        {
            WriteableBitmap mergedBitmap = null;
            var p1 = 0d;
            var p2 = 0d;

            var bitmaps = await Task.WhenAll(
                LoadWriteableBitmapAsync(uri1, new Progress<double>(p => { p1 = p; progress.Report((p1 + p2) / 2d); }), cancellationToken),
                LoadWriteableBitmapAsync(uri2, new Progress<double>(p => { p2 = p; progress.Report((p1 + p2) / 2d); }), cancellationToken));

            if (!cancellationToken.IsCancellationRequested &&
                bitmaps.Length == 2 &&
                bitmaps[0] != null &&
                bitmaps[1] != null &&
                bitmaps[0].PixelHeight == bitmaps[1].PixelHeight)
            {
                var buffer1 = bitmaps[0].PixelBuffer;
                var buffer2 = bitmaps[1].PixelBuffer;
                var stride1 = (uint)bitmaps[0].PixelWidth * 4;
                var stride2 = (uint)bitmaps[1].PixelWidth * 4;
                var stride = stride1 + stride2;
                var height = bitmaps[0].PixelHeight;

                mergedBitmap = new WriteableBitmap(bitmaps[0].PixelWidth + bitmaps[1].PixelWidth, height);

                var buffer = mergedBitmap.PixelBuffer;

                for (uint y = 0; y < height; y++)
                {
                    buffer1.CopyTo(y * stride1, buffer, y * stride, stride1);
                    buffer2.CopyTo(y * stride2, buffer, y * stride + stride1, stride2);
                }
            }

            return mergedBitmap;
        }
    }
}
