// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using MapControl.Projections;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl.Images
{
    public partial class GeoTaggedImage
    {
        private const string PixelScaleQuery = "/ifd/{ushort=33550}";
        private const string TiePointQuery = "/ifd/{ushort=33922}";
        private const string TransformQuery = "/ifd/{ushort=34264}";
        private const string NoDataQuery = "/ifd/{ushort=42113}";

        public static readonly DependencyProperty PathProperty = DependencyProperty.RegisterAttached(
            "Path", typeof(string), typeof(GeoTaggedImage), new PropertyMetadata(null, PathPropertyChanged));

        public BitmapSource Bitmap { get; }
        public Matrix Transform { get; }
        public MapProjection Projection { get; }
        public BoundingBox BoundingBox { get; }
        public double Rotation { get; }

        public GeoTaggedImage(BitmapSource bitmap, Matrix transform, MapProjection projection)
        {
            Bitmap = bitmap;
            Transform = transform;
            Projection = projection;

            if (transform.M12 != 0 || transform.M21 != 0)
            {
                Rotation = (Math.Atan2(transform.M12, transform.M11) + Math.Atan2(transform.M21, -transform.M22)) * 90d / Math.PI;

                // effective unrotated transform
                transform.M11 = Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12);
                transform.M22 = -Math.Sqrt(transform.M22 * transform.M22 + transform.M21 * transform.M21);
                transform.M12 = 0;
                transform.M21 = 0;
            }

            var rect = new Rect(
                transform.Transform(new Point()),
                transform.Transform(new Point(bitmap.PixelWidth, bitmap.PixelHeight)));

            BoundingBox = projection != null
                ? projection.RectToBoundingBox(rect)
                : new BoundingBox
                {
                    West = rect.X,
                    East = rect.X + rect.Width,
                    South = rect.Y,
                    North = rect.Y + rect.Height
                };
        }

        public static string GetPath(Image image)
        {
            return (string)image.GetValue(PathProperty);
        }

        public static void SetPath(Image image, string path)
        {
            image.SetValue(PathProperty, path);
        }

        public static Task<GeoTaggedImage> ReadImage(string imageFilePath)
        {
            var ext = Path.GetExtension(imageFilePath);
            if (ext.Length < 4)
            {
                throw new ArgumentException("Invalid image file path extension, must have at least three characters.");
            }

            var dir = Path.GetDirectoryName(imageFilePath);
            var file = Path.GetFileNameWithoutExtension(imageFilePath);
            var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");

            if (File.Exists(worldFilePath))
            {
                return ReadImage(imageFilePath, worldFilePath, Path.Combine(dir, file + ".prj"));
            }

            return ReadGeoTiff(imageFilePath);
        }

        public static async Task<GeoTaggedImage> ReadImage(string imageFilePath, string worldFilePath, string projFilePath = null)
        {
            var transform = ReadWorldFile(worldFilePath);

            var projection = (projFilePath != null && File.Exists(projFilePath))
                ? ReadProjectionFile(projFilePath)
                : null;

            var bitmap = (BitmapSource)await ImageLoader.LoadImageAsync(imageFilePath);

            return new GeoTaggedImage(bitmap, transform, projection);
        }

        public static Matrix ReadWorldFile(string path)
        {
            var parameters = File.ReadLines(path)
                .Take(6)
                .Select((line, i) =>
                {
                    if (!double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out double parameter))
                    {
                        throw new ArgumentException("Failed parsing line " + (i + 1) + " in world file \"" + path + "\".");
                    }
                    return parameter;
                })
                .ToList();

            if (parameters.Count != 6)
            {
                throw new ArgumentException("Insufficient number of parameters in world file \"" + path + "\".");
            }

            return new Matrix(
                parameters[0],  // line 1: A or M11
                parameters[1],  // line 2: D or M12
                parameters[2],  // line 3: B or M21
                parameters[3],  // line 4: E or M22
                parameters[4],  // line 5: C or OffsetX
                parameters[5]); // line 6: F or OffsetY
        }

        public static MapProjection ReadProjectionFile(string path)
        {
            return new GeoApiProjection { WKT = File.ReadAllText(path) };
        }

        public void ApplyToImage(Image image)
        {
            if (Rotation != 0d)
            {
                throw new InvalidOperationException("Rotation must be zero.");
            }

            image.Source = Bitmap;
            image.Stretch = Stretch.Fill;

            MapPanel.SetBoundingBox(image, BoundingBox);
        }

        public FrameworkElement CreateImage()
        {
            FrameworkElement image = new Image
            {
                Source = Bitmap,
                Stretch = Stretch.Fill
            };

            if (Rotation != 0d)
            {
                image.RenderTransform = new RotateTransform { Angle = Rotation };
                var panel = new Grid();
                panel.Children.Add(image);
                image = panel;
            }

            MapPanel.SetBoundingBox(image, BoundingBox);

            return image;
        }

        public static async Task<FrameworkElement> CreateImage(string imageFilePath)
        {
            return (await ReadImage(imageFilePath)).CreateImage();
        }

        private static async void PathPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Image image && e.NewValue is string imageFilePath)
            {
                (await ReadImage(imageFilePath)).ApplyToImage(image);
            }
        }
    }
}

