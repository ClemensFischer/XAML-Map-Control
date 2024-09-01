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
    public partial class GeoImage : Grid
    {
        private class DefaultProjection : MapProjection
        {
            public override Point? LocationToMap(Location location) => new Point(location.Longitude, location.Latitude);
            public override Location MapToLocation(Point point) => new Location(point.Y, point.X);
        }

        private class GeoBitmap
        {
            public BitmapSource Bitmap { get; set; }
            public Matrix Transform { get; set; }
            public MapProjection Projection { get; set; } = new DefaultProjection();

            public void SetProjection(short[] geoKeyDirectory)
            {
                for (var i = 4; i < geoKeyDirectory.Length - 3; i += 4)
                {
                    if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                    {
                        var epsgCode = geoKeyDirectory[i + 3];

                        var projection = MapProjectionFactory.Instance.GetProjection(epsgCode) ??
                            throw new ArgumentException($"Can not create projection EPSG:{epsgCode}.");

                        Projection = projection;
                        break;
                    }
                }
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
            DependencyPropertyHelper.Register<GeoImage, string>(nameof(SourcePath), null,
                async (image, oldValue, newValue) => await image.SourcePathPropertyChanged(newValue));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        private async Task SourcePathPropertyChanged(string sourcePath)
        {
            if (sourcePath != null)
            {
                try
                {
                    await ReadGeoImageAsync(sourcePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(GeoImage)}: {sourcePath}: {ex.Message}");
                }
            }
        }

        private async Task ReadGeoImageAsync(string sourcePath)
        {
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

#if AVALONIA
            if (geoBitmap == null) return;
            
            var width = geoBitmap.Bitmap.PixelSize.Width;
            var height = geoBitmap.Bitmap.PixelSize.Height;
#else
            if (geoBitmap == null)
            {
                geoBitmap = await ReadGeoTiffAsync(sourcePath);
            }

            var width = geoBitmap.Bitmap.PixelWidth;
            var height = geoBitmap.Bitmap.PixelHeight;
#endif
            var image = new Image
            {
                Source = geoBitmap.Bitmap,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var transform = geoBitmap.Transform;
            var p1 = transform.Transform(new Point());
            var p2 = transform.Transform(new Point(width, height));
            var mapRect = new Rect(p1, p2); ;

            MapPanel.SetBoundingBox(this, geoBitmap.Projection.MapToBoundingBox(mapRect));

            Children.Clear();
            Children.Add(image);
        }

        private static async Task<GeoBitmap> ReadWorldFileImageAsync(string sourcePath, string worldFilePath)
        {
            return new GeoBitmap
            {
                Bitmap = (BitmapSource)await ImageLoader.LoadImageAsync(sourcePath),
                Transform = await Task.Run(() => ReadWorldFileMatrix(worldFilePath))
            };
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
    }
}
