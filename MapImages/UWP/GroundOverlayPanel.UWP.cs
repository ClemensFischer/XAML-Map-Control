// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.IO;
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

        private static async Task<IEnumerable<ImageOverlay>> ReadGroundOverlaysFromFileAsync(string docFile)
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
    }
}
