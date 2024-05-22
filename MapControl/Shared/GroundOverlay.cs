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
using System.Xml;
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
using DependencyProperty = Avalonia.AvaloniaProperty;
using FrameworkElement = Avalonia.Controls.Control;
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl
{
    public class GroundOverlay : MapPanel
    {
        class LatLonBox : BoundingBox
        {
            public LatLonBox(double south, double west, double north, double east, double rotation)
                : base(south, west, north, east)
            {
                Rotation = rotation;
            }

            public double Rotation { get; }
        }

        class ImageOverlay
        {
            public ImageOverlay(LatLonBox latLonBox, string imagePath, int zIndex)
            {
                LatLonBox = latLonBox;
                ImagePath = imagePath;
                ZIndex = zIndex;
            }

            public LatLonBox LatLonBox { get; }
            public string ImagePath { get; }
            public int ZIndex { get; }
            public ImageSource ImageSource { get; set; }
        }

        public static readonly DependencyProperty SourcePathProperty =
            DependencyPropertyHelper.Register<GroundOverlay, string>(nameof(SourcePath), null, false,
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
                    Debug.WriteLine($"GroundOverlayPanel: {sourcePath}: {ex.Message}");
                }
            }

            Children.Clear();

            if (imageOverlays != null)
            {
                AddImageOverlays(imageOverlays);
            }
        }

        private void AddImageOverlays(IEnumerable<ImageOverlay> imageOverlays)
        {
            foreach (var imageOverlay in imageOverlays.Where(i => i.ImageSource != null))
            {
                FrameworkElement overlay = new Image
                {
                    Source = imageOverlay.ImageSource,
                    Stretch = Stretch.Fill
                };

                if (imageOverlay.LatLonBox.Rotation != 0d)
                {
                    SetRenderTransform(overlay, new RotateTransform { Angle = -imageOverlay.LatLonBox.Rotation }, 0.5, 0.5);

                    // Additional Panel for map rotation, see MapPanel.ArrangeElement(FrameworkElement, ViewRect).
                    //
                    var panel = new Grid();
                    panel.Children.Add(overlay);
                    overlay = panel;
                }

                SetBoundingBox(overlay, imageOverlay.LatLonBox);
                overlay.SetValue(Canvas.ZIndexProperty, imageOverlay.ZIndex);
                Children.Add(overlay);
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
                    var kmlDocument = new XmlDocument();

                    using (var docStream = docEntry.Open())
                    {
                        kmlDocument.Load(docStream);
                    }

                    return ReadGroundOverlays(kmlDocument).ToList();
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
                var kmlDocument = new XmlDocument();
                kmlDocument.Load(docFilePath);

                return ReadGroundOverlays(kmlDocument).ToList();
            });

            foreach (var imageOverlay in imageOverlays)
            {
                imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(new Uri(docUri, imageOverlay.ImagePath));
            }

            return imageOverlays;
        }

        private static IEnumerable<ImageOverlay> ReadGroundOverlays(XmlDocument kmlDocument)
        {
            foreach (XmlElement groundOverlayElement in kmlDocument.GetElementsByTagName("GroundOverlay"))
            {
                LatLonBox latLonBox = null;
                string imagePath = null;
                int zIndex = 0;

                foreach (var childElement in groundOverlayElement.ChildNodes.OfType<XmlElement>())
                {
                    switch (childElement.LocalName)
                    {
                        case "LatLonBox":
                            latLonBox = ReadLatLonBox(childElement);
                            break;
                        case "Icon":
                            imagePath = ReadImagePath(childElement);
                            break;
                        case "drawOrder":
                            zIndex = int.Parse(childElement.InnerText.Trim());
                            break;
                    }
                }

                if (latLonBox != null && imagePath != null)
                {
                    yield return new ImageOverlay(latLonBox, imagePath, zIndex);
                }
            }
        }

        private static string ReadImagePath(XmlElement element)
        {
            string href = null;

            foreach (var childElement in element.ChildNodes.OfType<XmlElement>())
            {
                switch (childElement.LocalName)
                {
                    case "href":
                        href = childElement.InnerText.Trim();
                        break;
                }
            }

            return href;
        }

        private static LatLonBox ReadLatLonBox(XmlElement element)
        {
            var north = double.NaN;
            var south = double.NaN;
            var east = double.NaN;
            var west = double.NaN;
            var rotation = 0d;

            foreach (var childElement in element.ChildNodes.OfType<XmlElement>())
            {
                switch (childElement.LocalName)
                {
                    case "north":
                        north = double.Parse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                    case "south":
                        south = double.Parse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                    case "east":
                        east = double.Parse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                    case "west":
                        west = double.Parse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                    case "rotation":
                        rotation = double.Parse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                }
            }

            if (north <= south || east <= west)
            {
                throw new FormatException("Invalid LatLonBox");
            }

            return new LatLonBox(south, west, north, east, rotation);
        }
    }
}
