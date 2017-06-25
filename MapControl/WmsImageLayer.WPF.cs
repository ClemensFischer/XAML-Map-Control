// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Xml;

namespace MapControl
{
    public partial class WmsImageLayer
    {
        private static async Task<XmlDocument> LoadDocument(Uri requestUri)
        {
            var document = new XmlDocument();
            await Task.Run(() => document.Load(requestUri.ToString()));
            return document;
        }
    }
}
