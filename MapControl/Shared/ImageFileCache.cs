// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// Image Cache implementation based on local image files.
    /// The only valid data type for cached values is Tuple<byte[], DateTime>.
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

            Debug.WriteLine($"Created ImageFileCache in {rootDirectory}");
        }

        public Task Clean()
        {
            return Task.Factory.StartNew(CleanRootDirectory, TaskCreationOptions.LongRunning);
        }

        private string GetPath(string key)
        {
            try
            {
                return Path.Combine(rootDirectory, Path.Combine(key.Split('/', ':', ';', ',')));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Invalid key {rootDirectory}/{key}: {ex.Message}");
            }

            return null;
        }

        private void CleanRootDirectory()
        {
            try
            {
                foreach (var dir in new DirectoryInfo(rootDirectory).EnumerateDirectories())
                {
                    var deletedFileCount = CleanDirectory(dir);

                    if (deletedFileCount > 0)
                    {
                        Debug.WriteLine($"ImageFileCache: Cleaned {deletedFileCount} files in {dir}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed enumerating directories in {rootDirectory}: {ex.Message}");
            }
        }

        private static int CleanDirectory(DirectoryInfo directory)
        {
            var deletedFileCount = 0;

            try
            {
                deletedFileCount += directory.EnumerateDirectories().Sum(dir => CleanDirectory(dir));

                deletedFileCount += directory.EnumerateFiles().Sum(file => CleanFile(file));

                if (!directory.EnumerateFileSystemInfos().Any())
                {
                    directory.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed cleaning {directory.FullName}: {ex.Message}");
            }

            return deletedFileCount;
        }

        private static int CleanFile(FileInfo file)
        {
            var deletedFileCount = 0;

            try
            {
                if (ReadExpiration(file) < DateTime.UtcNow)
                {
                    file.Delete();
                    deletedFileCount = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed cleaning {file.FullName}: {ex.Message}");
            }

            return deletedFileCount;
        }

        private static DateTime ReadExpiration(FileInfo file)
        {
            DateTime? expiration = null;

            if (file.Length > 16)
            {
                var buffer = new byte[16];

                using (var stream = file.OpenRead())
                {
                    stream.Seek(-16, SeekOrigin.End);

                    if (stream.Read(buffer, 0, 16) == 16)
                    {
                        expiration = ReadExpiration(buffer);
                    }
                }
            }

            return expiration ?? DateTime.Today;
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
            DateTime? expiration = null;

            if (buffer.Length >= 16 &&
                Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == expiresTag)
            {
                expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
            }

            return expiration;
        }

        private static void WriteExpiration(Stream stream, DateTime expiration)
        {
            stream.Write(Encoding.ASCII.GetBytes(expiresTag), 0, 8);
            stream.Write(BitConverter.GetBytes(expiration.Ticks), 0, 8);
        }

        private static async Task WriteExpirationAsync(Stream stream, DateTime expiration)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(expiresTag), 0, 8);
            await stream.WriteAsync(BitConverter.GetBytes(expiration.Ticks), 0, 8);
        }
    }
}
