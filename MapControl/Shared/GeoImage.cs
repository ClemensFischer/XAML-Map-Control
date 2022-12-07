// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#elif UWP
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

namespace MapControl
{
    public partial class GeoImage : ContentControl
    {
        private const string PixelScaleQuery = "/ifd/{ushort=33550}";
        private const string TiePointQuery = "/ifd/{ushort=33922}";
        private const string TransformQuery = "/ifd/{ushort=34264}";
        private const string NoDataQuery = "/ifd/{ushort=42113}";

        public static readonly DependencyProperty SourcePathProperty = DependencyProperty.Register(
            nameof(SourcePath), typeof(string), typeof(GeoImage),
            new PropertyMetadata(null, async (o, e) => await ((GeoImage)o).SourcePathPropertyChanged((string)e.NewValue)));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        public GeoImage()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        private async Task SourcePathPropertyChanged(string sourcePath)
        {
            Image image = null;
            BoundingBox boundingBox = null;

            if (sourcePath != null)
            {
                Tuple<BitmapSource, Matrix> geoBitmap = null;

                var ext = Path.GetExtension(sourcePath);

                if (ext.Length >= 4)
                {
                    var dir = Path.GetDirectoryName(sourcePath);
                    var file = Path.GetFileNameWithoutExtension(sourcePath);
                    var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");

                    if (File.Exists(worldFilePath))
                    {
                        geoBitmap = await ReadWorldFileImage(sourcePath, worldFilePath);
                    }
                }

                if (geoBitmap == null)
                {
                    geoBitmap = await ReadGeoTiff(sourcePath);
                }

                var bitmap = geoBitmap.Item1;
                var transform = geoBitmap.Item2;

                image = new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.Fill
                };

                if (transform.M12 != 0 || transform.M21 != 0)
                {
                    var rotation = (Math.Atan2(transform.M12, transform.M11) + Math.Atan2(transform.M21, -transform.M22)) * 90d / Math.PI;

                    image.RenderTransform = new RotateTransform { Angle = -rotation };

                    // Calculate effective unrotated transform.
                    //
                    transform.M11 = Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12);
                    transform.M22 = -Math.Sqrt(transform.M22 * transform.M22 + transform.M21 * transform.M21);
                    transform.M12 = 0;
                    transform.M21 = 0;
                }

                var p1 = transform.Transform(new Point());
                var p2 = transform.Transform(new Point(bitmap.PixelWidth, bitmap.PixelHeight));

                boundingBox = new BoundingBox(p1.Y, p1.X, p2.Y, p2.X); // Y=Latitude, X=Longitude
            }

            Content = image;

            MapPanel.SetBoundingBox(this, boundingBox);
        }

        private static async Task<Tuple<BitmapSource, Matrix>> ReadWorldFileImage(string sourcePath, string worldFilePath)
        {
            var bitmap = (BitmapSource)await ImageLoader.LoadImageAsync(sourcePath);

            var transform = await Task.Run(() =>
            {
                var parameters = File.ReadLines(worldFilePath)
                    .Take(6)
                    .Select((line, i) =>
                    {
                        if (!double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out double parameter))
                        {
                            throw new ArgumentException($"Failed parsing line {i + 1} in world file {worldFilePath}.");
                        }
                        return parameter;
                    })
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
            });

            return new Tuple<BitmapSource, Matrix>(bitmap, transform);
        }
    }
}
