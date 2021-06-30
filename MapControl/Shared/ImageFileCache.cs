// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MapControl.Caching
{
    /// <summary>
    /// Image Cache implementation based on local image files.
    /// The only valid data type for cached values is MapControl.ImageCacheItem.
    /// </summary>
    public partial class ImageFileCache
    {
        private const string expiresTag = "EXPIRES:";

        private readonly string rootDirectory;

        public ImageFileCache(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The directory argument must not be null or empty.", nameof(directory));
            }

            rootDirectory = directory;
            Debug.WriteLine("Created ImageFileCache in " + rootDirectory);
        }

        private string GetPath(string key)
        {
            try
            {
                return Path.Combine(rootDirectory, Path.Combine(key.Split('/', ':', ';', ',')));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}/{1}: {2}", rootDirectory, key, ex.Message);
            }

            return null;
        }

        private static DateTime ReadExpiration(ref byte[] buffer)
        {
            DateTime? expiration = ReadExpiration(buffer);

            if (expiration.HasValue)
            {
                Array.Resize(ref buffer, buffer.Length - 16);
                return expiration.Value;
            }

            return DateTime.Today;
        }

        private static DateTime? ReadExpiration(byte[] buffer)
        {
            if (buffer.Length >= 16 &&
                Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == expiresTag)
            {
                return new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
            }

            return null;
        }
    }
}
