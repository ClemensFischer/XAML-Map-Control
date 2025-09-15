using System.IO;

namespace MapControl
{
    public static class FilePath
    {
        public static string GetFullPath(string path)
        {
#if NETFRAMEWORK
            return Path.GetFullPath(path);
#else
            return Path.GetFullPath(path, System.AppDomain.CurrentDomain.BaseDirectory);
#endif
        }
    }
}
