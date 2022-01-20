// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class GeoImage
    {
        private static async Task<Tuple<BitmapSource, Matrix>> ReadGeoTiff(string sourcePath)
        {
            return await Task.Run(() =>
            {
                BitmapSource bitmap;
                Matrix transform;

                using (var stream = File.OpenRead(sourcePath))
                {
                    bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                var metadata = (BitmapMetadata)bitmap.Metadata;

                if (metadata.GetQuery(PixelScaleQuery) is double[] pixelScale && pixelScale.Length == 3 &&
                    metadata.GetQuery(TiePointQuery) is double[] tiePoint && tiePoint.Length >= 6)
                {
                    transform = new Matrix(pixelScale[0], 0d, 0d, -pixelScale[1], tiePoint[3], tiePoint[4]);
                }
                else if (metadata.GetQuery(TransformQuery) is double[] tform && tform.Length == 16)
                {
                    transform = new Matrix(tform[0], tform[1], tform[4], tform[5], tform[3], tform[7]);
                }
                else
                {
                    throw new ArgumentException($"No coordinate transformation found in {sourcePath}.");
                }

                if (metadata.GetQuery(NoDataQuery) is string noData && int.TryParse(noData, out int noDataValue))
                {
                    bitmap = ConvertTransparentPixel(bitmap, noDataValue);
                }

                return new Tuple<BitmapSource, Matrix>(bitmap, transform);
            });
        }

        private static BitmapSource ConvertTransparentPixel(BitmapSource source, int transparentPixel)
        {
            BitmapPalette sourcePalette = null;
            var targetFormat = source.Format;

            if (source.Format == PixelFormats.Indexed8 ||
                source.Format == PixelFormats.Indexed4 ||
                source.Format == PixelFormats.Indexed2 ||
                source.Format == PixelFormats.Indexed1)
            {
                sourcePalette = source.Palette;
            }
            else if (source.Format == PixelFormats.Gray8)
            {
                sourcePalette = BitmapPalettes.Gray256;
                targetFormat = PixelFormats.Indexed8;
            }
            else if (source.Format == PixelFormats.Gray4)
            {
                sourcePalette = BitmapPalettes.Gray16;
                targetFormat = PixelFormats.Indexed4;
            }
            else if (source.Format == PixelFormats.Gray2)
            {
                sourcePalette = BitmapPalettes.Gray4;
                targetFormat = PixelFormats.Indexed2;
            }
            else if (source.Format == PixelFormats.BlackWhite)
            {
                sourcePalette = BitmapPalettes.BlackAndWhite;
                targetFormat = PixelFormats.Indexed1;
            }

            if (sourcePalette == null || transparentPixel >= sourcePalette.Colors.Count)
            {
                return source;
            }

            var colors = sourcePalette.Colors.ToList();

            colors[transparentPixel] = Colors.Transparent;

            var stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            var buffer = new byte[stride * source.PixelHeight];

            source.CopyPixels(buffer, stride, 0);

            var target = BitmapSource.Create(
                source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY,
                targetFormat, new BitmapPalette(colors), buffer, stride);

            target.Freeze();

            return target;
        }
    }
}
