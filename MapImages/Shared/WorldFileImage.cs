// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using MapControl.Projections;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WINDOWS_UWP
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
    public class WorldFileImage
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.RegisterAttached(
            "Path", typeof(string), typeof(WorldFileImage),
            new PropertyMetadata(null, async (o, e) => (await ReadWorldFileImage((string)e.NewValue)).SetImage((Image)o)));

        public BitmapSource Bitmap { get; }
        public Matrix Transform { get; }
        public MapProjection Projection { get; }
        public BoundingBox BoundingBox { get; }
        public double Rotation { get; }

        public WorldFileImage(BitmapSource bitmap, Matrix transform, MapProjection projection)
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

        public static async Task<WorldFileImage> ReadWorldFileImage(string imagePath, string worldFilePath, string projFilePath = null)
        {
            var bitmap = (BitmapSource)await ImageLoader.LoadImageAsync(imagePath);
            var transform = ReadWorldFile(worldFilePath);
            var projection = (projFilePath != null && File.Exists(projFilePath))
                ? new GeoApiProjection { WKT = File.ReadAllText(projFilePath) }
                : null;

            return new WorldFileImage(bitmap, transform, projection);
        }

        public static Task<WorldFileImage> ReadWorldFileImage(string imagePath)
        {
            var ext = Path.GetExtension(imagePath);
            if (ext.Length < 4)
            {
                throw new ArgumentException("Invalid image file path extension, must have at least three characters.");
            }

            var dir = Path.GetDirectoryName(imagePath);
            var file = Path.GetFileNameWithoutExtension(imagePath);
            var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");
            var projFilePath = Path.Combine(dir, file + ".prj");

            return ReadWorldFileImage(imagePath, worldFilePath, projFilePath);
        }

        public static Matrix ReadWorldFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("World file \"" + path + "\"not found.");
            }

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

        public static MapProjection ReadProjFile(string path)
        {
            return new GeoApiProjection { WKT = File.ReadAllText(path) };
        }

        public void SetImage(Image image)
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

        public static async Task<FrameworkElement> CreateImage(string imagePath)
        {
            return (await ReadWorldFileImage(imagePath)).CreateImage();
        }
    }
}

