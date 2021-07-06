// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
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
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl.Images
{
    public class GroundOverlayPanel : MapPanel
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

        public static readonly DependencyProperty KmlFileProperty = DependencyProperty.Register(
            nameof(KmlFile), typeof(string), typeof(GroundOverlayPanel),
            new PropertyMetadata(null, async (o, e) => await ((GroundOverlayPanel)o).KmlFilePropertyChanged((string)e.NewValue)));

        public string KmlFile
        {
            get { return (string)GetValue(KmlFileProperty); }
            set { SetValue(KmlFileProperty, value); }
        }

        private async Task KmlFilePropertyChanged(string path)
        {
            IEnumerable<ImageOverlay> imageOverlays = null;

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var ext = Path.GetExtension(path).ToLower();
                    if (ext == ".kmz")
                    {
                        imageOverlays = await ReadGroundOverlaysFromArchiveAsync(path);
                    }
                    else if (ext == ".kml")
                    {
                        imageOverlays = await ReadGroundOverlaysFromFileAsync(path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GroundOverlayPanel: {path}: {ex.Message}");
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
                    Stretch = Stretch.Fill,
                    UseLayoutRounding = false
                };

                if (imageOverlay.LatLonBox.Rotation != 0d)
                {
                    overlay.RenderTransform = new RotateTransform { Angle = -imageOverlay.LatLonBox.Rotation };
                    overlay.RenderTransformOrigin = new Point(0.5, 0.5);

                    // additional Panel for map rotation, see MapPanel.ArrangeElementWithBoundingBox
                    var panel = new Grid { UseLayoutRounding = false };
                    panel.Children.Add(overlay);
                    overlay = panel;
                }

                SetBoundingBox(overlay, imageOverlay.LatLonBox);
                Canvas.SetZIndex(overlay, imageOverlay.ZIndex);
                Children.Add(overlay);
            }
        }

        private static async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromArchiveAsync(string archiveFile)
        {
            using (var archive = await Task.Run(() => ZipFile.OpenRead(archiveFile)))
            {
                var docEntry = await Task.Run(() => archive.GetEntry("doc.kml") ?? archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml")));

                if (docEntry == null)
                {
                    throw new ArgumentException("No KML entry found in " + archiveFile);
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
                    var imageEntry = await Task.Run(() => archive.GetEntry(imageOverlay.ImagePath));

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

        private static async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromFileAsync(string docFile)
        {
            docFile = Path.GetFullPath(docFile);
            var docUri = new Uri(docFile);

            var imageOverlays = await Task.Run(() =>
            {
                var kmlDocument = new XmlDocument();
                kmlDocument.Load(docFile);

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
                            int.TryParse(childElement.InnerText.Trim(), out zIndex);
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
            double north = double.NaN;
            double south = double.NaN;
            double east = double.NaN;
            double west = double.NaN;
            double rotation = 0d;

            foreach (var childElement in element.ChildNodes.OfType<XmlElement>())
            {
                switch (childElement.LocalName)
                {
                    case "north":
                        double.TryParse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out north);
                        break;
                    case "south":
                        double.TryParse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out south);
                        break;
                    case "east":
                        double.TryParse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out east);
                        break;
                    case "west":
                        double.TryParse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out west);
                        break;
                    case "rotation":
                        double.TryParse(childElement.InnerText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out rotation);
                        break;
                }
            }

            return !double.IsNaN(north) && !double.IsNaN(south) && !double.IsNaN(east) && !double.IsNaN(west)
                ? new LatLonBox(south, west, north, east, rotation)
                : null;
        }
    }
}
