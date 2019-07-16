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
        private static Task<List<ImageOverlay>> ReadGroundOverlaysFromFileAsync(string docFile)
        {
            return Task.Run(() =>
            {
                docFile = Path.GetFullPath(docFile);

                var kmlDocument = new XmlDocument();
                kmlDocument.Load(docFile);

                var imageOverlays = ReadGroundOverlays(kmlDocument).ToList();
                var docDir = Path.GetDirectoryName(docFile);

                foreach (var imageOverlay in imageOverlays)
                {
                    imageOverlay.ImageSource = ImageLoader.LoadImage(Path.Combine(docDir, imageOverlay.ImagePath));
                }

                return imageOverlays;
            });
        }

        private static Task<List<ImageOverlay>> ReadGroundOverlaysFromArchiveAsync(string archiveFile)
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
                                memoryStream.Seek(0, SeekOrigin.Begin);

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
