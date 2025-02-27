namespace MapControl
{
    public static class FilePath
    {
        public static string GetFullPath(string path)
        {
#if NET6_0_OR_GREATER
            return System.IO.Path.GetFullPath(path, System.AppDomain.CurrentDomain.BaseDirectory);
#else
            return System.IO.Path.GetFullPath(path);
#endif
        }
    }
}
