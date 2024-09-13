// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shape = System.Windows.Shapes.Shape;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Shape = Windows.UI.Xaml.Shapes.Shape;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Shape = Microsoft.UI.Xaml.Shapes.Shape;
#elif AVALONIA
using Shape = Avalonia.Controls.Shapes.Shape;
#endif

namespace MapControl
{
    public static partial class GeoImage
    {
        private partial class GeoBitmap
        {
            public BitmapSource BitmapSource { get; }
            public LatLonBox LatLonBox { get; }

            public GeoBitmap(BitmapSource bitmapSource, Matrix transform, MapProjection projection)
            {
                BitmapSource = bitmapSource;

                var p1 = transform.Transform(new Point());
                var p2 = transform.Transform(BitmapSize);

                LatLonBox = projection != null
                    ? new LatLonBox(projection.MapToBoundingBox(new Rect(p1, p2)))
                    : new LatLonBox(p1.Y, p1.X, p2.Y, p2.X);
            }
        }

        private const ushort ProjectedCRSGeoKey = 3072;
        private const ushort GeoKeyDirectoryTag = 34735;
        private const ushort ModelPixelScaleTag = 33550;
        private const ushort ModelTiePointTag = 33922;
        private const ushort ModelTransformationTag = 34264;
        private const ushort NoDataTag = 42113;

        private static string QueryString(ushort tag) => $"/ifd/{{ushort={tag}}}";

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.RegisterAttached<string>("SourcePath", typeof(GeoImage), null,
                async (element, oldValue, newValue) => await LoadGeoImageAsync(element, newValue));

        public static string GetSourcePath(FrameworkElement image)
        {
            return (string)image.GetValue(SourcePathProperty);
        }

        public static void SetSourcePath(FrameworkElement image, string value)
        {
            image.SetValue(SourcePathProperty, value);
        }

        public static async Task<Image> CreateAsync(string sourcePath)
        {
            var image = new Image();

            await LoadGeoImageAsync(image, sourcePath);

            return image;
        }

        public static async Task LoadGeoImageAsync(this FrameworkElement element, string sourcePath)
        {
            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var geoBitmap = await LoadGeoBitmapAsync(sourcePath);

                    if (element is Image image)
                    {
                        image.Source = geoBitmap.BitmapSource;
                        image.Stretch = Stretch.Fill;
                    }
                    else if (element is Shape shape)
                    {
                        shape.Fill = geoBitmap.ImageBrush;
                        shape.Stretch = Stretch.Fill;
                    }

                    MapPanel.SetBoundingBox(element, geoBitmap.LatLonBox);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(GeoImage)}: {sourcePath}: {ex.Message}");
                }
            }
        }

        private static async Task<GeoBitmap> LoadGeoBitmapAsync(string sourcePath)
        {
            var ext = Path.GetExtension(sourcePath);

            if (ext.Length >= 4)
            {
                var dir = Path.GetDirectoryName(sourcePath);
                var file = Path.GetFileNameWithoutExtension(sourcePath);
                var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");

                if (File.Exists(worldFilePath))
                {
                    return new GeoBitmap(
                        (BitmapSource)await ImageLoader.LoadImageAsync(sourcePath),
                        await ReadWorldFileMatrix(worldFilePath),
                        null);
                }
            }

            return await LoadGeoTiffAsync(sourcePath);
        }

        private static async Task<Matrix> ReadWorldFileMatrix(string worldFilePath)
        {
            using (var fileStream = File.OpenRead(worldFilePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                var parameters = new double[6];
                var index = 0;
                string line;

                while (index < 6 &&
                    (line = await streamReader.ReadLineAsync()) != null &&
                    double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out double parameter))
                {
                    parameters[index++] = parameter;
                }

                if (index != 6)
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
        }

        private static MapProjection GetProjection(short[] geoKeyDirectory)
        {
            for (var i = 4; i < geoKeyDirectory.Length - 3; i += 4)
            {
                if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                {
                    var epsgCode = geoKeyDirectory[i + 3];

                    return MapProjectionFactory.Instance.GetProjection($"EPSG:{epsgCode}");
                }
            }

            return null;
        }
    }
}
