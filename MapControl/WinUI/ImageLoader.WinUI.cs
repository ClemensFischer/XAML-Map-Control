using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace MapControl;

public static partial class ImageLoader
{
    public static ImageSource LoadResourceImage(Uri uri)
    {
        return new BitmapImage(uri);
    }

    public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream randomAccessStream)
    {
        var image = new BitmapImage();

        await image.SetSourceAsync(randomAccessStream);

        return image;
    }

    public static async Task<ImageSource> LoadImageAsync(Stream stream)
    {
        using var randomAccessStream = stream.AsRandomAccessStream();

        return await LoadImageAsync(randomAccessStream);
    }

    public static async Task<ImageSource> LoadImageAsync(string path)
    {
        ImageSource image = null;

        path = FilePath.GetFullPath(path);

        if (File.Exists(path))
        {
            var file = await StorageFile.GetFileFromPathAsync(path);

            using var randomAccessStream = await file.OpenReadAsync();

            image = await LoadImageAsync(randomAccessStream);
        }

        return image;
    }

    private class BitmapData(int width, int height, byte[] buffer)
    {
        public int Width => width;
        public int Height => height;
        public byte[] Buffer => buffer;
    }

    private static async Task<BitmapData> LoadBitmapData(Uri uri, IProgress<double> progress)
    {
        BitmapData data = null;

        progress.Report(0d);

        try
        {
            var buffer = await GetHttpContent(uri, progress);

            if (buffer != null)
            {
                using var memoryStream = new MemoryStream(buffer);
                using var randomAccessStream = memoryStream.AsRandomAccessStream();

                var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                    ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

                data = new BitmapData((int)decoder.PixelWidth, (int)decoder.PixelHeight, pixelData.DetachPixelData());
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed loading {uri}", uri);
        }

        progress.Report(1d);

        return data;
    }

    internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
    {
        WriteableBitmap mergedImage = null;
        var p1 = 0d;
        var p2 = 0d;

        var bitmaps = await Task.WhenAll(
            LoadBitmapData(uri1, new Progress<double>(p => { p1 = p; progress.Report((p1 + p2) / 2d); })),
            LoadBitmapData(uri2, new Progress<double>(p => { p2 = p; progress.Report((p1 + p2) / 2d); })));

        if (bitmaps.Length == 2 &&
            bitmaps[0] != null &&
            bitmaps[1] != null &&
            bitmaps[0].Height == bitmaps[1].Height)
        {
            var height = bitmaps[0].Height;
            var stride1 = bitmaps[0].Width * 4;
            var stride2 = bitmaps[1].Width * 4;
            var stride = stride1 + stride2;
            var buffer1 = bitmaps[0].Buffer;
            var buffer2 = bitmaps[1].Buffer;

            mergedImage = new WriteableBitmap(bitmaps[0].Width + bitmaps[1].Width, height);
            var buffer = mergedImage.PixelBuffer;

            for (int y = 0; y < height; y++)
            {
                buffer1.CopyTo(y * stride1, buffer, (uint)(y * stride), stride1);
                buffer2.CopyTo(y * stride2, buffer, (uint)(y * stride + stride1), stride2);
            }
        }

        return mergedImage;
    }
}
