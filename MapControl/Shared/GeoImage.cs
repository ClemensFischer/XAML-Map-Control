// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public partial class GeoImage
    {
        private const ushort ProjectedCRSGeoKey = 3072;
        private const ushort GeoKeyDirectoryTag = 34735;
        private const ushort ModelPixelScaleTag = 33550;
        private const ushort ModelTiePointTag = 33922;
        private const ushort ModelTransformationTag = 34264;
        private const ushort NoDataTag = 42113;

        private static string QueryString(ushort tag) => $"/ifd/{{ushort={tag}}}";

        private BitmapSource bitmapSource;
        private Matrix transformMatrix;
        private MapProjection mapProjection;
        private BoundingBox boundingBox;

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.RegisterAttached<GeoImage, string>("SourcePath", null,
                async (image, oldValue, newValue) => await LoadGeoImageAsync((Image)image, newValue));

        public static string GetSourcePath(Image image)
        {
            return (string)image.GetValue(SourcePathProperty);
        }

        public static void SetSourcePath(Image image, string value)
        {
            image.SetValue(SourcePathProperty, value);
        }

        public static Image LoadGeoImage(string sourcePath)
        {
            var image = new Image();

            SetSourcePath(image, sourcePath);

            return image;
        }

        private static async Task LoadGeoImageAsync(Image image, string sourcePath)
        {
            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var geoImage = new GeoImage();

                    await geoImage.LoadGeoImageAsync(sourcePath);

                    image.Source = geoImage.bitmapSource;
                    image.Stretch = Stretch.Fill;

                    MapPanel.SetBoundingBox(image, geoImage.boundingBox);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(GeoImage)}: {sourcePath}: {ex.Message}");
                }
            }
        }

        private async Task LoadGeoImageAsync(string sourcePath)
        {
            var ext = Path.GetExtension(sourcePath);

            if (ext.Length >= 4)
            {
                var dir = Path.GetDirectoryName(sourcePath);
                var file = Path.GetFileNameWithoutExtension(sourcePath);
                var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");

                if (File.Exists(worldFilePath))
                {
                    await LoadWorldFileImageAsync(sourcePath, worldFilePath);
                }
            }

            if (bitmapSource == null)
            {
                await LoadGeoTiffAsync(sourcePath);
            }

            var p1 = transformMatrix.Transform(new Point());
            var p2 = transformMatrix.Transform(BitmapSize);

            boundingBox = mapProjection != null
                ? mapProjection.MapToBoundingBox(new Rect(p1, p2))
                : new BoundingBox(p1.Y, p1.X, p2.Y, p2.X);
        }

        private async Task LoadWorldFileImageAsync(string sourcePath, string worldFilePath)
        {
            transformMatrix = await Task.Run(() => ReadWorldFileMatrix(worldFilePath));

            bitmapSource = (BitmapSource)await ImageLoader.LoadImageAsync(sourcePath);
        }

        private static Matrix ReadWorldFileMatrix(string worldFilePath)
        {
            var parameters = File.ReadLines(worldFilePath)
                .Select(line => double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out double p) ? (double?)p : null)
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .Take(6)
                .ToList();

            if (parameters.Count != 6)
            {
                throw new ArgumentException($"Insufficient number of parameters in world file {worldFilePath}.");
            }

            return new Matrix(
                parameters[0],  // line 1: A or M11
                parameters[1],  // line 2: D or M12
                parameters[2],  // line 3: B or M21
                parameters[3],  // line 4: E or M22
                parameters[4],  // line 5: C or OffsetX
                parameters[5]); // line 6: F or OffsetY
        }

        private void SetProjection(short[] geoKeyDirectory)
        {
            for (var i = 4; i < geoKeyDirectory.Length - 3; i += 4)
            {
                if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                {
                    var epsgCode = geoKeyDirectory[i + 3];

                    mapProjection = MapProjectionFactory.Instance.GetProjection(epsgCode) ??
                        throw new ArgumentException($"Can not create projection EPSG:{epsgCode}.");
                }
            }
        }
    }
}
