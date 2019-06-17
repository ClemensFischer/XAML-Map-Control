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
using Windows.Storage;

namespace MapControl.Images
{
    public partial class GroundOverlayPanel
    {
        public GroundOverlayPanel()
        {
            UseLayoutRounding = false;
        }

        private async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromFileAsync(string docFile)
        {
            docFile = Path.GetFullPath(docFile);

            var file = await StorageFile.GetFileFromPathAsync(docFile);
            var kmlDocument = new XmlDocument();

            using (var stream = await file.OpenReadAsync())
            {
                kmlDocument.Load(stream.AsStreamForRead());
            }

            var imageOverlays = await Task.Run(() => ReadGroundOverlays(kmlDocument).ToList());
            var docUri = new Uri(docFile);

            foreach (var imageOverlay in imageOverlays)
            {
                imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(new Uri(docUri, imageOverlay.ImagePath));
            }

            return imageOverlays;
        }

        private async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromArchiveAsync(string archiveFile)
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

                var imageOverlays = await Task.Run(() => ReadGroundOverlays(kmlDocument).ToList());

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
                            imageOverlay.ImageSource = await ImageLoader.LoadImageAsync(memoryStream.AsRandomAccessStream());
                        }
                    }
                }

                return imageOverlays;
            }
        }
    }
}
