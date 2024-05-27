// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
