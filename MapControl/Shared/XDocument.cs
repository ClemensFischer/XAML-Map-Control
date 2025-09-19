using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MapControl
{
    internal static class XDocument
    {
        public static async Task<XElement> LoadRootElementAsync(Stream stream)
        {
#if NETFRAMEWORK
            var document = await Task.Run(() => System.Xml.Linq.XDocument.Load(stream, LoadOptions.None));
#else
            var document = await System.Xml.Linq.XDocument.LoadAsync(stream, LoadOptions.None, System.Threading.CancellationToken.None);
#endif
            return document.Root;
        }
    }
}
