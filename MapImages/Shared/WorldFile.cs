// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
using MapControl.Projections;

namespace MapControl.Images
{
    public static class WorldFile
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.RegisterAttached(
            "ImagePath", typeof(string), typeof(WorldFile),
            new PropertyMetadata(null, async (o, e) => await SetWorldImageAsync((Image)o, (string)e.NewValue)));

        public static string GetImagePath(this Image image)
        {
            return (string)image.GetValue(ImagePathProperty);
        }

        public static void SetImagePath(this Image image, string imagePath)
        {
            image.SetValue(ImagePathProperty, imagePath);
        }

        public static void SetWorldImage(this Image image, BitmapSource bitmapSource, Matrix transform, MapProjection projection = null)
        {
            image.Source = bitmapSource;
            image.Stretch = Stretch.Fill;

            var boundingBox = GetBoundingBox(bitmapSource.PixelWidth, bitmapSource.PixelHeight, transform, projection);

            MapPanel.SetBoundingBox(image, boundingBox);
        }

        public static BoundingBox GetBoundingBox(double imageWidth, double imageHeight, Matrix transform, MapProjection projection = null)
        {
            if (transform.M12 != 0d || transform.M21 != 0d)
            {
                throw new ArgumentException("Invalid Matrix, M12 and M21 must be zero.");
            }

            var rect = new Rect(
                transform.Transform(new Point()),
                transform.Transform(new Point(imageWidth, imageHeight)));

            if (projection != null)
            {
                return projection.RectToBoundingBox(rect);
            }

            return new BoundingBox
            {
                West = rect.X,
                East = rect.X + rect.Width,
                South = rect.Y,
                North = rect.Y + rect.Height
            };
        }

        public static Matrix ReadWorldFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("World file \"" + path + "\"not found.");
            }

            var parameters = File.ReadLines(path).Take(6).Select((line, i) =>
            {
                double p;
                if (!double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out p))
                {
                    throw new ArgumentException("Failed parsing line " + (i + 1) + " in world file \"" + path + "\".");
                }
                return p;
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

        private static async Task SetWorldImageAsync(Image image, string imagePath)
        {
            var ext = Path.GetExtension(imagePath);
            if (ext.Length < 4)
            {
                throw new ArgumentException("Invalid image file path extension, must have at least three characters.");
            }

            BitmapSource bitmap;
            using (var stream = File.OpenRead(imagePath))
            {
#if WINDOWS_UWP
                bitmap = await ImageLoader.LoadImageAsync(stream.AsRandomAccessStream());
#else
                bitmap = await ImageLoader.LoadImageAsync(stream);
#endif
            }

            var dir = Path.GetDirectoryName(imagePath);
            var file = Path.GetFileNameWithoutExtension(imagePath);
            var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");
            var projFilePath = Path.Combine(dir, file + ".prj");

            var transform = ReadWorldFile(worldFilePath);
            MapProjection projection = null;

            if (File.Exists(projFilePath))
            {
                projection = new GeoApiProjection { WKT = File.ReadAllText(projFilePath) };
            }

            SetWorldImage(image, bitmap, transform, projection);
        }
    }
}
