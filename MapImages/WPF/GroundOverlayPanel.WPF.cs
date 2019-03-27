// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MapControl.Images
{
    public partial class GroundOverlayPanel
    {
        private async Task<List<ImageOverlay>> ReadGroundOverlaysFromFile(string docFile)
        {
            if (!Path.IsPathRooted(docFile))
            {
                docFile = Path.Combine(Directory.GetCurrentDirectory(), docFile);
            }

            var docUri = new Uri(docFile);

            var imageOverlays = await Task.Run(() =>
            {
                var kmlDocument = new XmlDocument();
                kmlDocument.Load(docUri.ToString());

                return ReadGroundOverlays(kmlDocument).ToList();
            });

            foreach (var imageOverlay in imageOverlays)
            {
                imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(new Uri(docUri, imageOverlay.ImagePath));
            }

            return imageOverlays;
        }

        private Task<List<ImageOverlay>> ReadGroundOverlaysFromArchive(string archiveFile)
        {
            return Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(archiveFile))
                {
                    var docEntry = archive.GetEntry("doc.kml")
                        ?? archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml"));

                    if (docEntry == null)
                    {
                        throw new ArgumentException("No KML entry found in " + archiveFile);
                    }

                    var kmlDocument = new XmlDocument();

                    using (var docStream = docEntry.Open())
                    {
                        kmlDocument.Load(docStream);
                    }

                    var imageOverlays = ReadGroundOverlays(kmlDocument).ToList();

                    foreach (var imageOverlay in imageOverlays)
                    {
                        var imageEntry = archive.GetEntry(imageOverlay.ImagePath);

                        if (imageEntry != null)
                        {
                            using (var zipStream = imageEntry.Open())
                            using (var memoryStream = new MemoryStream())
                            {
                                zipStream.CopyTo(memoryStream);
                                memoryStream.Position = 0;
                                imageOverlay.ImageSource = ImageLoader.LoadImage(memoryStream);
                            }
                        }
                    }

                    return imageOverlays;
                }
            });
        }
    }
}
