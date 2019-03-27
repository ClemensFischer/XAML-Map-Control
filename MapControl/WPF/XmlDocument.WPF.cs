// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    internal class XmlDocument : System.Xml.XmlDocument
    {
        public static XmlDocument LoadFromUri(Uri uri)
        {
            var document = new XmlDocument();
            document.Load(uri.ToString());
            return document;
        }

        public static Task<XmlDocument> LoadFromUriAsync(Uri uri)
        {
            return Task.Run(() => LoadFromUri(uri));
        }
    }
}
