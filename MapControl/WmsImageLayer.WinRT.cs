// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace MapControl
{
    public partial class WmsImageLayer
    {
        private static async Task<XmlDocument> LoadDocument(string requestUri)
        {
            return await XmlDocument.LoadFromUriAsync(new Uri(requestUri));
        }
    }
}
