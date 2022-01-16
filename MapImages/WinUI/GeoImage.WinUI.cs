// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.Storage;
#if WINUI
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace MapControl.Images
{
    public partial class GeoImage
    {
        public static async Task<GeoImage> ReadGeoTiff(string imageFilePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(imageFilePath));

            using (var stream = await file.OpenReadAsync())
            {
                WriteableBitmap bitmap;
                Matrix transform;

                var decoder = await BitmapDecoder.CreateAsync(stream);

                using (var swbmp = await decoder.GetSoftwareBitmapAsync())
                {
                    bitmap = new WriteableBitmap(swbmp.PixelWidth, swbmp.PixelHeight);
                    swbmp.CopyToBuffer(bitmap.PixelBuffer);
                }

                var query = new List<string>
                {
                    PixelScaleQuery, TiePointQuery, TransformQuery //, NoDataQuery
                };

                var metadata = await decoder.BitmapProperties.GetPropertiesAsync(query);

                if (metadata.TryGetValue(PixelScaleQuery, out BitmapTypedValue pixelScaleValue) &&
                    pixelScaleValue.Value is double[] pixelScale && pixelScale.Length == 3 &&
                    metadata.TryGetValue(TiePointQuery, out BitmapTypedValue tiePointValue) &&
                    tiePointValue.Value is double[] tiePoint && tiePoint.Length >= 6)
                {
                    transform = new Matrix(pixelScale[0], 0d, 0d, -pixelScale[1], tiePoint[3], tiePoint[4]);
                }
                else if (metadata.TryGetValue(TransformQuery, out BitmapTypedValue tformValue) &&
                         tformValue.Value is double[] tform && tform.Length == 16)
                {
                    transform = new Matrix(tform[0], tform[1], tform[4], tform[5], tform[3], tform[7]);
                }
                else
                {
                    throw new ArgumentException("No coordinate transformation found in \"" + imageFilePath + "\".");
                }

                return new GeoImage(bitmap, transform, null);
            }
        }
    }
}
