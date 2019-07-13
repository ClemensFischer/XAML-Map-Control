// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.IO;
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
    }
}
