﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
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
            public ImageOverlay(string path, LatLonBox latLonBox, int zIndex)
            {
                ImagePath = path;
                SetBoundingBox(Image, latLonBox);
                Image.SetValue(Canvas.ZIndexProperty, zIndex);
            }

            public string ImagePath { get; }

            public Image Image { get; } = new Image { Stretch = Stretch.Fill };

            public async Task LoadImage(Uri docUri)
            {
                Image.Source = await ImageLoader.LoadImageAsync(new Uri(docUri, ImagePath));
            }

            public async Task LoadImage(ZipArchive archive)
            {
                var entry = archive.GetEntry(ImagePath);

                if (entry != null)
                {
                    using (var memoryStream = new MemoryStream((int)entry.Length))
                    {
                        using (var zipStream = entry.Open())
                        {
                            zipStream.CopyTo(memoryStream); // can't use CopyToAsync with ZipArchive
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        Image.Source = await ImageLoader.LoadImageAsync(memoryStream);
                    }
                }
            }
        }

        private static ILogger logger;
        private static ILogger Logger => logger ?? (logger = ImageLoader.LoggerFactory?.CreateLogger<GroundOverlay>());

        public static int MaxLoadTasks { get; set; } = 4;

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
            List<ImageOverlay> imageOverlays = null;

            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var ext = Path.GetExtension(sourcePath).ToLower();

                    if (ext == ".kmz")
                    {
                        imageOverlays = await LoadImageOverlaysFromArchive(sourcePath);
                    }
                    else if (ext == ".kml")
                    {
                        imageOverlays = await LoadImageOverlaysFromFile(sourcePath);
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
                foreach (var imageOverlay in imageOverlays)
                {
                    Children.Add(imageOverlay.Image);
                }
            }
        }

        private static async Task<List<ImageOverlay>> LoadImageOverlaysFromArchive(string archiveFilePath)
        {
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                var docEntry = archive.GetEntry("doc.kml") ??
                               archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml")) ??
                               throw new ArgumentException($"No KML entry found in {archiveFilePath}.");
                XDocument document;

                using (var docStream = docEntry.Open())
                {
                    document = await LoadXDocument(docStream);
                }

                return await LoadImageOverlays(document, imageOverlay => imageOverlay.LoadImage(archive));
            }
        }

        private static async Task<List<ImageOverlay>> LoadImageOverlaysFromFile(string docFilePath)
        {
            var docUri = new Uri(FilePath.GetFullPath(docFilePath));
            XDocument document;

            using (var docStream = File.OpenRead(docUri.AbsolutePath))
            {
                document = await LoadXDocument(docStream);
            }

            return await LoadImageOverlays(document, imageOverlay => imageOverlay.LoadImage(docUri));
        }

        private static async Task<List<ImageOverlay>> LoadImageOverlays(XDocument document, Func<ImageOverlay, Task> loadFunc)
        {
            var imageOverlays = ReadImageOverlays(document);

            using (var semaphore = new SemaphoreSlim(MaxLoadTasks))
            {
                var tasks = imageOverlays.Select(
                    async imageOverlay =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await loadFunc(imageOverlay); // no more than MaxLoadTasks parallel executions here
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                await Task.WhenAll(tasks);
            }

            return imageOverlays;
        }

        private static List<ImageOverlay> ReadImageOverlays(XDocument document)
        {
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
                    var zIndex = drawOrder != null ? int.Parse(drawOrder) : 0;

                    if (latLonBox != null && path != null)
                    {
                        imageOverlays.Add(new ImageOverlay(path, latLonBox, zIndex));
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

        private static Task<XDocument> LoadXDocument(Stream docStream)
        {
#if NETFRAMEWORK
            return Task.Run(() => XDocument.Load(docStream, LoadOptions.None));
#else
            return XDocument.LoadAsync(docStream, LoadOptions.None, System.Threading.CancellationToken.None);
#endif
        }
    }
}
