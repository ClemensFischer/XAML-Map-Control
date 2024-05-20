// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.IO;

namespace MapControl
{
    public static class FilePath
    {
        public static string GetFullPath(string path)
        {
#if NET6_0_OR_GREATER
            return Path.GetFullPath(path, System.AppDomain.CurrentDomain.BaseDirectory);
#else
            return Path.GetFullPath(path);
#endif
        }
    }
}
