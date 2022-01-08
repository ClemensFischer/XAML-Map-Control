// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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

                if (mdata.GetQuery(PixelScaleQuery) is double[] pixelScale &&
                    mdata.GetQuery(TiePointQuery) is double[] tiePoint &&
                    pixelScale.Length == 3 && tiePoint.Length >= 6)
                {
                    transform = new Matrix(pixelScale[0], 0d, 0d, -pixelScale[1], tiePoint[3], tiePoint[4]);
                }
                else if (mdata.GetQuery(TransformationQuery) is double[] transformation &&
                         transformation.Length == 16)
                {
                    transform = new Matrix(transformation[0], transformation[1], transformation[4],
                                           transformation[5], transformation[3], transformation[7]);
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
            var target = source;

            if (source.Format == PixelFormats.Gray8 && transparentPixel < 256)
            {
                var colors = Enumerable.Range(0, 256)
                    .Select(i => Color.FromArgb(i == transparentPixel ? (byte)0 : (byte)255, (byte)i, (byte)i, (byte)i))
                    .ToList();

                var buffer = new byte[source.PixelWidth * source.PixelHeight];

                source.CopyPixels(buffer, source.PixelWidth, 0);

                target = BitmapSource.Create(
                    source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY,
                    PixelFormats.Indexed8, new BitmapPalette(colors), buffer, source.PixelWidth);

                target.Freeze();
            }

            return target;
        }
    }
}
