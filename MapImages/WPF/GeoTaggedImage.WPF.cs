// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl.Images
{
    public partial class GeoTaggedImage
    {
        private const string PixelScaleQuery = "/ifd/{ushort=33550}";
        private const string TiePointQuery = "/ifd/{ushort=33922}";
        private const string TransformationQuery = "/ifd/{ushort=34264}";
        private const string NoDataQuery = "/ifd/{ushort=42113}";

        public static Task<GeoTaggedImage> ReadGeoTiff(string imageFilePath)
        {
            return Task.Run(() =>
            {
                BitmapSource bitmap;
                Matrix transform;

                using (var stream = File.OpenRead(imageFilePath))
                {
                    bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                var mdata = bitmap.Metadata as BitmapMetadata;

                if (mdata.GetQuery(PixelScaleQuery) is double[] ps &&
                    mdata.GetQuery(TiePointQuery) is double[] tp &&
                    ps.Length == 3 && tp.Length >= 6)
                {
                    transform = new Matrix(ps[0], 0d, 0d, -ps[1], tp[3], tp[4]);
                }
                else if (mdata.GetQuery(TransformationQuery) is double[] tf && tf.Length == 16)
                {
                    transform = new Matrix(tf[0], tf[1], tf[4], tf[5], tf[3], tf[7]);
                }
                else
                {
                    throw new ArgumentException("No coordinate transformation found in \"" + imageFilePath + "\".");
                }

                if (mdata.GetQuery(NoDataQuery) is string noData && int.TryParse(noData, out int noDataValue))
                {
                    bitmap = ConvertTransparentPixel(bitmap, noDataValue);
                }

                return new GeoTaggedImage(bitmap, transform, null);
            });
        }

        public static BitmapSource ConvertTransparentPixel(BitmapSource source, int transparentPixel)
        {
            List<Color> colors = null;
            var format = source.Format;
            var bpp = format.BitsPerPixel;

            if (format == PixelFormats.Indexed8 ||
                format == PixelFormats.Indexed4 ||
                format == PixelFormats.Indexed2)
            {
                colors = source.Palette.Colors.ToList();
            }
            else if (format == PixelFormats.Gray8 ||
                format == PixelFormats.Gray4 ||
                format == PixelFormats.Gray2)
            {
                format = bpp == 8 ? PixelFormats.Indexed8
                    : bpp == 4 ? PixelFormats.Indexed4 : PixelFormats.Indexed2;

                colors = Enumerable.Range(0, (1 << bpp))
                    .Select(i => Color.FromRgb((byte)i, (byte)i, (byte)i)).ToList();
            }

            var target = source;

            if (colors != null && transparentPixel < colors.Count)
            {
                colors[transparentPixel] = Colors.Transparent;

                var stride = (source.PixelWidth * bpp + 7) / 8;
                var buffer = new byte[stride * source.PixelHeight];

                source.CopyPixels(buffer, stride, 0);

                target = BitmapSource.Create(
                    source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY,
                    format, new BitmapPalette(colors), buffer, stride);

                target.Freeze();
            }

            return target;
        }
    }
}
