// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace MapControl
{
    /// <summary>
    /// Creates frozen BitmapSources from Stream, file or Uri.
    /// </summary>
    public static class BitmapSourceHelper
    {
        public static BitmapSource FromStream(Stream stream)
        {
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        public static BitmapSource FromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return FromStream(fileStream);
            }
        }

        public static BitmapSource FromUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return FromFile(uri.OriginalString);
            }

            if (uri.Scheme == "file")
            {
                return FromFile(uri.LocalPath);
            }

            using (var response = WebRequest.Create(uri).GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var memoryStream = new MemoryStream())
            {
                responseStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return FromStream(memoryStream);
            }
        }
    }
}
