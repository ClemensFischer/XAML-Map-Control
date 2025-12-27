using Microsoft.Extensions.Logging;
using System;
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shape = Avalonia.Controls.Shapes.Shape;
using BitmapSource = Avalonia.Media.Imaging.Bitmap;
#endif

namespace MapControl
{
    public static partial class GeoImage
    {
        private class GeoBitmap
        {
            public GeoBitmap(BitmapSource bitmap, Matrix transform, MapProjection projection)
            {
                var p1 = transform.Transform(new Point());
#if AVALONIA
                var p2 = transform.Transform(new Point(bitmap.PixelSize.Width, bitmap.PixelSize.Height));
#else
                var p2 = transform.Transform(new Point(bitmap.PixelWidth, bitmap.PixelHeight));
#endif
                BitmapSource = bitmap;
                LatLonBox = projection != null
                    ? new LatLonBox(projection.MapToBoundingBox(new Rect(p1, p2)))
                    : new LatLonBox(p1.Y, p1.X, p2.Y, p2.X);
            }

            public BitmapSource BitmapSource { get; }
            public LatLonBox LatLonBox { get; }
        }

        private const ushort ProjectedCRSGeoKey = 3072;
        private const ushort GeoKeyDirectoryTag = 34735;
        private const ushort ModelPixelScaleTag = 33550;
        private const ushort ModelTiePointTag = 33922;
        private const ushort ModelTransformationTag = 34264;
        private const ushort NoDataTag = 42113;

        private static string QueryString(ushort tag) => $"/ifd/{{ushort={tag}}}";

        private static ILogger Logger => field ??= ImageLoader.LoggerFactory?.CreateLogger(typeof(GeoImage));

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.RegisterAttached<string>("SourcePath", typeof(GeoImage), null,
                async (element, oldValue, newValue) => await LoadGeoImage(element, newValue));

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

            await LoadGeoImage(image, sourcePath);

            return image;
        }

        public static Task LoadGeoImageAsync(this Image image, string sourcePath)
        {
            return LoadGeoImage(image, sourcePath);
        }

        public static Task LoadGeoImageAsync(this Shape shape, string sourcePath)
        {
            return LoadGeoImage(shape, sourcePath);
        }

        private static async Task LoadGeoImage(FrameworkElement element, string sourcePath)
        {
            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var geoBitmap = await LoadGeoBitmap(sourcePath);

                    if (element is Image image)
                    {
                        image.Stretch = Stretch.Fill;
                        image.Source = geoBitmap.BitmapSource;
                    }
                    else if (element is Shape shape)
                    {
                        shape.Stretch = Stretch.Fill;
                        shape.Fill = new ImageBrush
                        {
                            Stretch = Stretch.Fill,
#if AVALONIA
                            Source = geoBitmap.BitmapSource
#else
                            ImageSource = geoBitmap.BitmapSource
#endif
                        };
                    }

                    MapPanel.SetBoundingBox(element, geoBitmap.LatLonBox);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed loading from {path}", sourcePath);
                }
            }
        }

        private static async Task<GeoBitmap> LoadGeoBitmap(string sourcePath)
        {
            var ext = System.IO.Path.GetExtension(sourcePath);

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

            return await LoadGeoTiff(sourcePath);
        }

        private static async Task<Matrix> ReadWorldFileMatrix(string worldFilePath)
        {
            using var fileStream = File.OpenRead(worldFilePath);
            using var streamReader = new StreamReader(fileStream);

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

        private static MapProjection GetProjection(short[] geoKeyDirectory)
        {
            for (var i = 4; i < geoKeyDirectory.Length - 3; i += 4)
            {
                if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                {
                    var epsgCode = geoKeyDirectory[i + 3];

                    return MapProjection.Parse($"EPSG:{epsgCode}");
                }
            }

            return null;
        }
    }
}
