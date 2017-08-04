// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

#pragma warning disable CS1998 // async method lacks await operators and will run synchronously

namespace MapControl
{
    public class XmlDocument : System.Xml.XmlDocument
    {
        public static Task<XmlDocument> LoadFromUriAsync(Uri uri)
        {
            return Task.Run(async () => // without async, debugger may stop on unhandled exception from XmlDocument.Load
            {
                var document = new XmlDocument();
                document.Load(uri.ToString());
                return document;
            });
        }
    }
}
