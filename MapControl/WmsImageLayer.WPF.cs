// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;
using System.Xml;

namespace MapControl
{
    public partial class WmsImageLayer
    {
        private static async Task<XmlDocument> LoadDocument(string requestUri)
        {
            var document = new XmlDocument();
            await Task.Run(() => document.Load(requestUri));
            return document;
        }
    }
}
