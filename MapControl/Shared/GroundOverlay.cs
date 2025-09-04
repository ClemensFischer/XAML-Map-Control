using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
#endif

namespace MapControl
{
    public class GroundOverlay : MapPanel
    {
        private class ImageOverlay
        {
            private readonly string imagePath;
            private readonly LatLonBox boundingBox;
            private readonly int zIndex;
            private ImageSource imageSource;

            public ImageOverlay(string path, LatLonBox latLonBox, int zOrder)
            {
                imagePath = path;
                boundingBox = latLonBox;
                zIndex = zOrder;
            }

            public async Task LoadImageSource(ZipArchive archive)
            {
                var entry = archive.GetEntry(imagePath);

                if (entry != null)
                {
                    MemoryStream memoryStream;

                    // ZipArchive does not support multithreading, synchronously copy ZipArchiveEntry stream to MemoryStream.
                    //
                    using (var zipStream = entry.Open())
                    {
                        memoryStream = new MemoryStream((int)zipStream.Length);
                        zipStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                    }

                    // Close Zip Stream before awaiting.
                    //
                    imageSource = await ImageLoader.LoadImageAsync(memoryStream);
                }
            }

            public async Task LoadImageSource(Uri docUri)
            {
                imageSource = await ImageLoader.LoadImageAsync(new Uri(docUri, imagePath));
            }

            public Image CreateImage()
            {
                Image image = null;

                if (imageSource != null)
                {
                    image = new Image
                    {
                        Source = imageSource,
                        Stretch = Stretch.Fill
                    };

                    image.SetValue(Canvas.ZIndexProperty, zIndex);
                    SetBoundingBox(image, boundingBox);
                }

                return image;
            }
        }

        private static ILogger logger;
        private static ILogger Logger => logger ?? (logger = ImageLoader.LoggerFactory?.CreateLogger<GroundOverlay>());

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.Register<GroundOverlay, string>(nameof(SourcePath), null,
                async (groundOverlay, oldValue, newValue) => await groundOverlay.LoadAsync(newValue));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        public static async Task<GroundOverlay> CreateAsync(string sourcePath)
        {
            var groundOverlay = new GroundOverlay();

            await groundOverlay.LoadAsync(sourcePath);

            return groundOverlay;
        }

        public async Task LoadAsync(string sourcePath)
        {
            IEnumerable<ImageOverlay> imageOverlays = null;

            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var ext = Path.GetExtension(sourcePath).ToLower();

                    if (ext == ".kmz")
                    {
                        imageOverlays = await LoadGroundOverlaysFromArchive(sourcePath);
                    }
                    else if (ext == ".kml")
                    {
                        imageOverlays = await LoadGroundOverlaysFromFile(sourcePath);
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed loading from {path}", sourcePath);
                }
            }

            Children.Clear();

            if (imageOverlays != null)
            {
                foreach (var image in imageOverlays
                    .Select(imageOverlay => imageOverlay.CreateImage())
                    .Where(image => image != null))
                {
                    Children.Add(image);
                }
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> LoadGroundOverlaysFromArchive(string archiveFilePath)
        {
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                var docEntry = archive.GetEntry("doc.kml") ??
                               archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml")) ??
                               throw new ArgumentException($"No KML entry found in {archiveFilePath}.");

                List<ImageOverlay> imageOverlays;

                using (var docStream = docEntry.Open())
                {
                    imageOverlays = await ReadGroundOverlays(docStream);
                }

                await Task.WhenAll(imageOverlays.Select(imageOverlay => imageOverlay.LoadImageSource(archive)));

                return imageOverlays;
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> LoadGroundOverlaysFromFile(string docFilePath)
        {
            docFilePath = FilePath.GetFullPath(docFilePath);

            List<ImageOverlay> imageOverlays;

            using (var docStream = File.OpenRead(docFilePath))
            {
                imageOverlays = await ReadGroundOverlays(docStream);
            }

            var docUri = new Uri(docFilePath);

            await Task.WhenAll(imageOverlays.Select(imageOverlay => imageOverlay.LoadImageSource(docUri)));

            return imageOverlays;
        }

        private static async Task<List<ImageOverlay>> ReadGroundOverlays(Stream docStream)
        {
#if NETFRAMEWORK
            var document = await Task.Run(() => XDocument.Load(docStream, LoadOptions.None));
#else
            var document = await XDocument.LoadAsync(docStream, LoadOptions.None, System.Threading.CancellationToken.None);
#endif
            var rootElement = document.Root;
            var ns = rootElement.Name.Namespace;
            var docElement = rootElement.Element(ns + "Document") ?? rootElement;
            var imageOverlays = new List<ImageOverlay>();

            foreach (var folderElement in docElement.Elements(ns + "Folder"))
            {
                foreach (var groundOverlayElement in folderElement.Elements(ns + "GroundOverlay"))
                {
                    var pathElement = groundOverlayElement.Element(ns + "Icon");
                    var path = pathElement?.Element(ns + "href")?.Value;

                    var latLonBoxElement = groundOverlayElement.Element(ns + "LatLonBox");
                    var latLonBox = latLonBoxElement != null ? ReadLatLonBox(latLonBoxElement) : null;

                    var drawOrder = groundOverlayElement.Element(ns + "drawOrder")?.Value;
                    var zOrder = drawOrder != null ? int.Parse(drawOrder) : 0;

                    if (latLonBox != null && path != null)
                    {
                        imageOverlays.Add(new ImageOverlay(path, latLonBox, zOrder));
                    }
                }
            }

            return imageOverlays;
        }

        private static LatLonBox ReadLatLonBox(XElement latLonBoxElement)
        {
            var ns = latLonBoxElement.Name.Namespace;
            var north = double.NaN;
            var south = double.NaN;
            var east = double.NaN;
            var west = double.NaN;
            var rotation = 0d;

            var value = latLonBoxElement.Element(ns + "north")?.Value;
            if (value != null)
            {
                north = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            value = latLonBoxElement.Element(ns + "south")?.Value;
            if (value != null)
            {
                south = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            value = latLonBoxElement.Element(ns + "east")?.Value;
            if (value != null)
            {
                east = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            value = latLonBoxElement.Element(ns + "west")?.Value;
            if (value != null)
            {
                west = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            value = latLonBoxElement.Element(ns + "rotation")?.Value;
            if (value != null)
            {
                rotation = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            if (double.IsNaN(north) || double.IsNaN(south) ||
                double.IsNaN(east) || double.IsNaN(west) ||
                north <= south || east <= west)
            {
                throw new FormatException("Invalid LatLonBox");
            }

            return new LatLonBox(south, west, north, east, rotation);
        }
    }
}
