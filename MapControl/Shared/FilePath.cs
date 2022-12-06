// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;

namespace MapControl
{
    public static class FilePath
    {
        public static string GetFullPath(string path)
        {
#if WINUI
            return Path.GetFullPath(path, AppDomain.CurrentDomain.BaseDirectory);
#else
            return Path.GetFullPath(path);
#endif
        }
    }
}
