// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        class ImageOverlay
        {
            public ImageOverlay(BoundingBox boundingBox, string imagePath, int zIndex)
            {
                BoundingBox = boundingBox;
                ImagePath = imagePath;
                ZIndex = zIndex;
            }

            public BoundingBox BoundingBox { get; }
            public string ImagePath { get; }
            public int ZIndex { get; }
            public ImageSource ImageSource { get; set; }
        }

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.Register<GroundOverlay, string>(nameof(SourcePath), null,
                async (overlay, oldValue, newValue) => await overlay.SourcePathPropertyChanged(newValue));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        private async Task SourcePathPropertyChanged(string sourcePath)
        {
            IEnumerable<ImageOverlay> imageOverlays = null;

            if (!string.IsNullOrEmpty(sourcePath))
            {
                try
                {
                    var ext = Path.GetExtension(sourcePath).ToLower();

                    if (ext == ".kmz")
                    {
                        imageOverlays = await ReadGroundOverlaysFromArchiveAsync(sourcePath);
                    }
                    else if (ext == ".kml")
                    {
                        imageOverlays = await ReadGroundOverlaysFromFileAsync(sourcePath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(GroundOverlay)}: {sourcePath}: {ex.Message}");
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
                    SetBoundingBox(image, imageOverlay.BoundingBox);
                    Children.Add(image);
                }
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromArchiveAsync(string archiveFilePath)
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

        private static async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromFileAsync(string docFilePath)
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
                    var boundingBoxElement = groundOverlayElement.Element(ns + "LatLonBox");
                    var boundingBox = boundingBoxElement != null ? ReadBoundingBox(boundingBoxElement) : null;

                    var imagePathElement = groundOverlayElement.Element(ns + "Icon");
                    var imagePath = imagePathElement?.Element(ns + "href")?.Value;

                    var drawOrder = groundOverlayElement.Element(ns + "drawOrder")?.Value;
                    var zIndex = drawOrder != null ? int.Parse(drawOrder) : 0;

                    if (boundingBox != null && imagePath != null)
                    {
                        yield return new ImageOverlay(boundingBox, imagePath, zIndex);
                    }
                }
            }
        }

        private static BoundingBox ReadBoundingBox(XElement latLonBoxElement)
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

            return new BoundingBox(south, west, north, east, rotation);
        }
    }
}
