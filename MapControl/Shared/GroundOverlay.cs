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
#endif

namespace MapControl
{
    public class GroundOverlay : MapPanel
    {
        private class ImageOverlay
        {
            public ImageOverlay(string imagePath, LatLonBox latLonBox, int zIndex)
            {
                ImagePath = imagePath;
                LatLonBox = latLonBox;
                ZIndex = zIndex;
            }

            public string ImagePath { get; }
            public LatLonBox LatLonBox { get; }
            public int ZIndex { get; }
            public ImageSource ImageSource { get; set; }
        }

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
                    ImageLoader.LoggerFactory?.CreateLogger<GroundOverlay>()?.LogError(ex, "Failed loading from {path}", sourcePath);
                }
            }

            Children.Clear();

            if (imageOverlays != null)
            {
                foreach (var imageOverlay in imageOverlays.Where(i => i.ImageSource != null))
                {
                    var image = new Image
                    {
                        Source = imageOverlay.ImageSource,
                        Stretch = Stretch.Fill
                    };

                    image.SetValue(Canvas.ZIndexProperty, imageOverlay.ZIndex);
                    SetBoundingBox(image, imageOverlay.LatLonBox);
                    Children.Add(image);
                }
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> LoadGroundOverlaysFromArchive(string archiveFilePath)
        {
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                var docEntry = archive.GetEntry("doc.kml") ??
                               archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml"));

                if (docEntry == null)
                {
                    throw new ArgumentException($"No KML entry found in {archiveFilePath}.");
                }

                var imageOverlays = await Task.Run(() =>
                {
                    using (var docStream = docEntry.Open())
                    {
                        var kmlDocument = XDocument.Load(docStream);

                        return ReadGroundOverlays(kmlDocument.Root).ToList();
                    }
                });

                foreach (var imageOverlay in imageOverlays)
                {
                    var imageEntry = archive.GetEntry(imageOverlay.ImagePath);

                    if (imageEntry != null)
                    {
                        using (var zipStream = imageEntry.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            await zipStream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(memoryStream);
                        }
                    }
                }

                return imageOverlays;
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> LoadGroundOverlaysFromFile(string docFilePath)
        {
            docFilePath = FilePath.GetFullPath(docFilePath);

            var docUri = new Uri(docFilePath);

            var imageOverlays = await Task.Run(() =>
            {
                var kmlDocument = XDocument.Load(docFilePath);

                return ReadGroundOverlays(kmlDocument.Root).ToList();
            });

            foreach (var imageOverlay in imageOverlays)
            {
                imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(new Uri(docUri, imageOverlay.ImagePath));
            }

            return imageOverlays;
        }

        private static IEnumerable<ImageOverlay> ReadGroundOverlays(XElement kmlElement)
        {
            var ns = kmlElement.Name.Namespace;
            var docElement = kmlElement.Element(ns + "Document") ?? kmlElement;

            foreach (var folderElement in docElement.Elements(ns + "Folder"))
            {
                foreach (var groundOverlayElement in folderElement.Elements(ns + "GroundOverlay"))
                {
                    var imagePathElement = groundOverlayElement.Element(ns + "Icon");
                    var imagePath = imagePathElement?.Element(ns + "href")?.Value;

                    var latLonBoxElement = groundOverlayElement.Element(ns + "LatLonBox");
                    var latLonBox = latLonBoxElement != null ? ReadLatLonBox(latLonBoxElement) : null;

                    var drawOrder = groundOverlayElement.Element(ns + "drawOrder")?.Value;
                    var zIndex = drawOrder != null ? int.Parse(drawOrder) : 0;

                    if (latLonBox != null && imagePath != null)
                    {
                        yield return new ImageOverlay(imagePath, latLonBox, zIndex);
                    }
                }
            }
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
