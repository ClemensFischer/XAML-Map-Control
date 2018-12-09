// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using MapControl.Projections;
#if WINDOWS_UWP
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

        public static void SetWorldImage(this Image image, BitmapSource bitmapSource, WorldFileParameters parameters, MapProjection projection = null)
        {
            image.Source = bitmapSource;
            image.Stretch = Stretch.Fill;

            MapPanel.SetBoundingBox(image, parameters.GetBoundingBox(bitmapSource.PixelWidth, bitmapSource.PixelHeight, projection));
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

            var parameters = new WorldFileParameters(worldFilePath);
            MapProjection projection = null;

            if (File.Exists(projFilePath))
            {
                projection = new GeoApiProjection { WKT = File.ReadAllText(projFilePath) };
            }

            SetWorldImage(image, bitmap, parameters, projection);
        }

        public static FrameworkElement CreateWorldImage(BitmapSource bitmap, WorldFileParameters parameters)
        {
            if (parameters.XScale == 0d || parameters.YScale == 0d)
            {
                throw new ArgumentException("Invalid WorldFileParameters, XScale and YScale must be non-zero.");
            }

            var pixelWidth = parameters.XScale;
            var pixelHeight = parameters.YScale;
            var rotation = 0d;

            if (parameters.YSkew != 0 || parameters.XSkew != 0)
            {
                pixelWidth = Math.Sqrt(parameters.XScale * parameters.XScale + parameters.YSkew * parameters.YSkew);
                pixelHeight = Math.Sqrt(parameters.YScale * parameters.YScale + parameters.XSkew * parameters.XSkew);

                var xAxisRotation = Math.Atan2(parameters.YSkew, parameters.XScale) / Math.PI * 180d;
                var yAxisRotation = Math.Atan2(parameters.XSkew, -parameters.YScale) / Math.PI * 180d;
                rotation = 0.5 * (xAxisRotation + yAxisRotation);
            }

            var x1 = parameters.XOrigin;
            var x2 = parameters.XOrigin + pixelWidth * bitmap.PixelWidth;
            var y1 = parameters.YOrigin;
            var y2 = parameters.YOrigin + pixelHeight * bitmap.PixelHeight;

            var bbox = new BoundingBox
            {
                West = Math.Min(x1, x2),
                East = Math.Max(x1, x2),
                South = Math.Min(y1, y2),
                North = Math.Max(y1, y2)
            };

            FrameworkElement image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Fill
            };

            if (rotation != 0d)
            {
                image.RenderTransform = new RotateTransform { Angle = rotation };
                var panel = new Grid();
                panel.Children.Add(image);
                image = panel;
            }

            MapPanel.SetBoundingBox(image, bbox);
            return image;
        }
    }
}
