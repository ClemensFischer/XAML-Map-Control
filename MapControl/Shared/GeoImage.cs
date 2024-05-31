// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.IO;
using Path = System.IO.Path;
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
    public partial class GeoImage : Grid
    {
        private class GeoBitmap
        {
            public BitmapSource Bitmap { get; set; }
            public Matrix Transform { get; set; }
            public MapProjection Projection { get; set; }
        }

        private const ushort ProjectedCRSGeoKey = 3072;
        private const ushort GeoKeyDirectoryTag = 34735;
        private const ushort ModelPixelScaleTag = 33550;
        private const ushort ModelTiePointTag = 33922;
        private const ushort ModelTransformationTag = 34264;
        private const ushort NoDataTag = 42113;

        private static string QueryString(ushort tag) => $"/ifd/{{ushort={tag}}}";

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.Register<GeoImage, string>(nameof(SourcePath), null,
                async (image, oldValue, newValue) => await image.SourcePathPropertyChanged(newValue));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        private async Task SourcePathPropertyChanged(string sourcePath)
        {
            if (sourcePath == null)
            {
                return;
            }

            GeoBitmap geoBitmap = null;
            var ext = Path.GetExtension(sourcePath);

            if (ext.Length >= 4)
            {
                var dir = Path.GetDirectoryName(sourcePath);
                var file = Path.GetFileNameWithoutExtension(sourcePath);
                var worldFilePath = Path.Combine(dir, file + ext.Remove(2, 1) + "w");

                if (File.Exists(worldFilePath))
                {
                    geoBitmap = await ReadWorldFileImageAsync(sourcePath, worldFilePath);
                }
            }

            if (geoBitmap == null)
            {
#if AVALONIA
                return;
#else
                geoBitmap = await ReadGeoTiffAsync(sourcePath);
#endif
            }

            var image = new Image
            {
                Source = geoBitmap.Bitmap,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var transform = geoBitmap.Transform;

            if (transform.M12 != 0 && transform.M21 != 0)
            {
                var rotation = (Math.Atan2(transform.M12, transform.M11) + Math.Atan2(transform.M21, -transform.M22)) * 90d / Math.PI;

                image.RenderTransform = new RotateTransform { Angle = -rotation };

                // Calculate effective unrotated transform.
                //
                geoBitmap.Transform = new Matrix(
                    Math.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12), 0d, 0d,
                    -Math.Sqrt(transform.M22 * transform.M22 + transform.M21 * transform.M21), 0d, 0d);
            }

#if AVALONIA
            var size = new Point(geoBitmap.Bitmap.PixelSize.Width, geoBitmap.Bitmap.PixelSize.Height);
#else
            var size = new Point(geoBitmap.Bitmap.PixelWidth, geoBitmap.Bitmap.PixelHeight);
#endif
            var rect = new Rect(transform.Transform(new Point()), transform.Transform(size));

            var boundingBox = geoBitmap.Projection != null
                ? geoBitmap.Projection.MapToBoundingBox(rect)
                : new BoundingBox(rect.Y, rect.X, rect.Y + rect.Height, rect.X + rect.Width);

            MapPanel.SetBoundingBox(this, boundingBox);

            Children.Clear();
            Children.Add(image);
        }

        private static async Task<GeoBitmap> ReadWorldFileImageAsync(string sourcePath, string worldFilePath)
        {
            var geoBitmap = new GeoBitmap();

            geoBitmap.Bitmap = (BitmapSource)await ImageLoader.LoadImageAsync(sourcePath);

            geoBitmap.Transform = await Task.Run(() =>
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

            return geoBitmap;
        }

        private static MapProjection GetProjection(string sourcePath, short[] geoKeyDirectory)
        {
            MapProjection projection = null;

            for (int i = 4; i < geoKeyDirectory.Length - 3; i += 4)
            {
                if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                {
                    int epsgCode = geoKeyDirectory[i + 3];

                    projection = MapProjectionFactory.Instance.GetProjection(epsgCode) ??
                        throw new ArgumentException($"Can not create projection EPSG:{epsgCode} in {sourcePath}.");

                    break;
                }
            }

            return projection;
        }
    }
}
