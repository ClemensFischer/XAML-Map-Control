// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
#if UWP
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class GeoImage
    {
        private Point BitmapSize => new Point(bitmapSource.PixelWidth, bitmapSource.PixelHeight);

        private ImageBrush ImageBrush => new ImageBrush { ImageSource = bitmapSource };

        private async Task LoadGeoTiffAsync(string sourcePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(FilePath.GetFullPath(sourcePath));

            using (var stream = await file.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);

                bitmapSource = await ImageLoader.LoadImageAsync(decoder);

                var geoKeyDirectoryQuery = QueryString(GeoKeyDirectoryTag);
                var pixelScaleQuery = QueryString(ModelPixelScaleTag);
                var tiePointQuery = QueryString(ModelTiePointTag);
                var transformationQuery = QueryString(ModelTransformationTag);
                var metadata = await decoder.BitmapProperties.GetPropertiesAsync(
                    new string[]
                    {
                        pixelScaleQuery,
                        tiePointQuery,
                        transformationQuery,
                        geoKeyDirectoryQuery
                    });

                if (metadata.TryGetValue(pixelScaleQuery, out BitmapTypedValue pixelScaleValue) &&
                    pixelScaleValue.Value is double[] pixelScale &&
                    pixelScale.Length == 3 &&
                    metadata.TryGetValue(tiePointQuery, out BitmapTypedValue tiePointValue) &&
                    tiePointValue.Value is double[] tiePoint &&
                    tiePoint.Length >= 6)
                {
                    transformMatrix = new Matrix(pixelScale[0], 0d, 0d, -pixelScale[1], tiePoint[3], tiePoint[4]);
                }
                else if (metadata.TryGetValue(transformationQuery, out BitmapTypedValue transformValue) &&
                         transformValue.Value is double[] transformValues &&
                         transformValues.Length == 16)
                {
                    transformMatrix = new Matrix(transformValues[0], transformValues[1],
                                                 transformValues[4], transformValues[5],
                                                 transformValues[3], transformValues[7]);
                }
                else
                {
                    throw new ArgumentException("No coordinate transformation found.");
                }

                if (metadata.TryGetValue(geoKeyDirectoryQuery, out BitmapTypedValue geoKeyDirValue) &&
                    geoKeyDirValue.Value is short[] geoKeyDirectory)
                {
                    SetProjection(geoKeyDirectory);
                }
            }
        }
    }
}
