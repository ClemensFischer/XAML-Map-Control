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
using System.Windows.Media;
using System.Windows.Media.Imaging;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public partial class GeoImage
    {
        private class GeoBitmap
        {
            public BitmapSource Bitmap { get; set; }
            public Matrix Transform { get; set; }
            public MapProjection Projection { get; set; }

            public BoundingBox BoundingBox
            {
                get
                {
                    var p1 = Transform.Transform(new Point());
#if AVALONIA
                    var p2 = Transform.Transform(new Point(Bitmap.PixelSize.Width, Bitmap.PixelSize.Height));
#else
                    var p2 = Transform.Transform(new Point(Bitmap.PixelWidth, Bitmap.PixelHeight));
#endif
                    return Projection != null
                        ? Projection.MapToBoundingBox(new Rect(p1, p2))
                        : new BoundingBox(p1.Y, p1.X, p2.Y, p2.X);
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

            if (geoBitmap == null)
            {
                geoBitmap = await ReadGeoTiffAsync(sourcePath);
            }

            MapPanel.SetBoundingBox(this, geoBitmap.BoundingBox);

            SetImage(geoBitmap.Bitmap);
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

        private static MapProjection GetProjection(short[] geoKeyDirectory)
        {
            for (var i = 4; i < geoKeyDirectory.Length - 3; i += 4)
            {
                if (geoKeyDirectory[i] == ProjectedCRSGeoKey && geoKeyDirectory[i + 1] == 0)
                {
                    var epsgCode = geoKeyDirectory[i + 3];

                    return MapProjectionFactory.Instance.GetProjection(epsgCode) ??
                        throw new ArgumentException($"Can not create projection EPSG:{epsgCode}.");
                }
            }

            return null;
        }
    }
}
