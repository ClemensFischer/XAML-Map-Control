// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on local image files.
    /// </summary>
    public class ImageFileCache : IDistributedCache
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

        public byte[] Get(string key)
        {
            byte[] buffer = null;
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    buffer = File.ReadAllBytes(path);

                    CheckExpiration(path, ref buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
            }

            return buffer;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            byte[] buffer = null;
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
#if NETFRAMEWORK
                    using (var stream = File.OpenRead(path))
                    {
                        buffer = new byte[stream.Length];
                        var offset = 0;
                        while (offset < buffer.Length)
                        {
                            offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
                        }
                    }
#else
                    buffer = await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
#endif
                    CheckExpiration(path, ref buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
            }

            return buffer;
        }

        public void Set(string key, byte[] buffer, DistributedCacheEntryOptions options)
        {
            var path = GetPath(key);

            if (path != null && buffer != null && buffer.Length > 0)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        stream.Write(buffer, 0, buffer.Length);

                        var expiration = GetExpiration(options);

                        if (expiration.HasValue)
                        {
                            stream.Write(Encoding.ASCII.GetBytes(expiresTag), 0, 8);
                            stream.Write(BitConverter.GetBytes(expiration.Value.Ticks), 0, 8);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
            }
        }

        public async Task SetAsync(string key, byte[] buffer, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var path = GetPath(key);

            if (path != null && buffer != null && buffer.Length > 0)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                        var expiration = GetExpiration(options);

                        if (expiration.HasValue)
                        {
                            await stream.WriteAsync(Encoding.ASCII.GetBytes(expiresTag), 0, 8).ConfigureAwait(false);
                            await stream.WriteAsync(BitConverter.GetBytes(expiration.Value.Ticks), 0, 8).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
            }
        }

        public void Refresh(string key)
        {
            throw new NotSupportedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotSupportedException();
        }

        public void Remove(string key)
        {
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed deleting {path}: {ex.Message}");
            }

        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        private string GetPath(string key)
        {
            try
            {
                return Path.Combine(rootDirectory, Path.Combine(key.Split('/')));
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
                var expiration = ReadExpiration(file);

                if (expiration.HasValue && expiration.Value <= DateTimeOffset.UtcNow)
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

        private static DateTimeOffset? GetExpiration(DistributedCacheEntryOptions options)
        {
            DateTimeOffset? expiration = null;

            if (options.AbsoluteExpiration.HasValue)
            {
                expiration = options.AbsoluteExpiration.Value;
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                expiration = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.SlidingExpiration.HasValue)
            {
                expiration = DateTimeOffset.UtcNow.Add(options.SlidingExpiration.Value);
            }

            return expiration;
        }

        private static void CheckExpiration(string path, ref byte[] buffer)
        {
            var expiration = ReadExpiration(ref buffer);

            if (expiration.HasValue && expiration.Value <= DateTimeOffset.UtcNow)
            {
                File.Delete(path);
                buffer = null;
            }
        }

        private static DateTimeOffset? ReadExpiration(FileInfo file)
        {
            DateTimeOffset? expiration = null;

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

            return expiration;
        }

        private static DateTimeOffset? ReadExpiration(ref byte[] buffer)
        {
            var expiration = ReadExpiration(buffer);

            if (expiration.HasValue)
            {
                Array.Resize(ref buffer, buffer.Length - 16);
            }

            return expiration;
        }

        private static DateTimeOffset? ReadExpiration(byte[] buffer)
        {
            DateTimeOffset? expiration = null;

            if (buffer.Length >= 16 &&
                Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == expiresTag)
            {
                expiration = new DateTimeOffset(BitConverter.ToInt64(buffer, buffer.Length - 8), TimeSpan.Zero);
            }

            return expiration;
        }
    }
}
