// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on local image files.
    /// </summary>
    public sealed class ImageFileCache : IDistributedCache, IDisposable
    {
        private readonly MemoryDistributedCache memoryCache;
        private readonly DirectoryInfo rootDirectory;
        private readonly Timer expirationScanTimer;
        private bool scanningExpiration;

        public ImageFileCache(string path)
            : this(path, TimeSpan.FromHours(1))
        {
        }

        public ImageFileCache(string path, TimeSpan expirationScanFrequency)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The {nameof(path)} argument must not be null or empty.", nameof(path));
            }

            rootDirectory = new DirectoryInfo(path);
            rootDirectory.Create();

            Debug.WriteLine($"{nameof(ImageFileCache)}: {rootDirectory.FullName}");

            var options = new MemoryDistributedCacheOptions();

            if (expirationScanFrequency > TimeSpan.Zero)
            {
                options.ExpirationScanFrequency = expirationScanFrequency;

                expirationScanTimer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, expirationScanFrequency);
            }

            memoryCache = new MemoryDistributedCache(Options.Create(options));
        }

        public void Dispose()
        {
            expirationScanTimer?.Dispose();
        }

        public byte[] Get(string key)
        {
            var buffer = memoryCache.Get(key);

            if (buffer == null)
            {
                var file = GetFile(key);

                try
                {
                    if (file != null && file.Exists && file.CreationTime > DateTime.Now)
                    {
                        buffer = ReadAllBytes(file);

                        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.CreationTime };

                        memoryCache.Set(key, buffer, options);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed reading {file.FullName}: {ex.Message}");
                }
            }

            return buffer;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var buffer = await memoryCache.GetAsync(key, token).ConfigureAwait(false);

            if (buffer == null)
            {
                var file = GetFile(key);

                try
                {
                    if (file != null && file.Exists && file.CreationTime > DateTime.Now && !token.IsCancellationRequested)
                    {
                        buffer = await ReadAllBytes(file, token).ConfigureAwait(false);

                        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.CreationTime };

                        await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    buffer = null;
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed reading {file.FullName}: {ex.Message}");
                }
            }

            return buffer;
        }

        public void Set(string key, byte[] buffer, DistributedCacheEntryOptions options)
        {
            memoryCache.Set(key, buffer, options);

            var file = GetFile(key);

            try
            {
                if (file != null && buffer?.Length > 0)
                {
                    file.Directory.Create();

                    using (var stream = file.Create())
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }

                    SetExpiration(file, options);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed writing {file.FullName}: {ex.Message}");
            }
        }

        public async Task SetAsync(string key, byte[] buffer, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);

            var file = GetFile(key);

            try
            {
                if (file != null && buffer?.Length > 0 && !token.IsCancellationRequested)
                {
                    file.Directory.Create();

                    using (var stream = file.Create())
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    }

                    SetExpiration(file, options);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed writing {file.FullName}: {ex.Message}");
            }
        }

        public void Refresh(string key)
        {
            memoryCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return memoryCache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            memoryCache.Remove(key);

            var file = GetFile(key);

            try
            {
                if (file != null && file.Exists)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed deleting {file.FullName}: {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await memoryCache.RemoveAsync(key, token);

            var file = GetFile(key);

            try
            {
                if (file != null && file.Exists && !token.IsCancellationRequested)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed deleting {file.FullName}: {ex.Message}");
            }
        }

        public void DeleteExpiredItems()
        {
            if (!scanningExpiration)
            {
                scanningExpiration = true;

                foreach (var directory in rootDirectory.EnumerateDirectories())
                {
                    var deletedFileCount = ScanDirectory(directory);

                    if (deletedFileCount > 0)
                    {
                        Debug.WriteLine($"{nameof(ImageFileCache)}: Deleted {deletedFileCount} expired items in {directory.Name}.");
                    }
                }

                scanningExpiration = false;
            }
        }

        private FileInfo GetFile(string key)
        {
            try
            {
                return new FileInfo(Path.Combine(rootDirectory.FullName, Path.Combine(key.Split('/'))));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Invalid key {key}: {ex.Message}");
            }

            return null;
        }

        private static byte[] ReadAllBytes(FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                var buffer = new byte[stream.Length];
                var offset = 0;

                while (offset < buffer.Length)
                {
                    offset += stream.Read(buffer, offset, buffer.Length - offset);
                }

                return buffer;
            }
        }

        private static async Task<byte[]> ReadAllBytes(FileInfo file, CancellationToken token)
        {
            using (var stream = file.OpenRead())
            {
                var buffer = new byte[stream.Length];
                var offset = 0;

                while (offset < buffer.Length)
                {
                    offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
                }

                return buffer;
            }
        }

        private static void SetExpiration(FileInfo file, DistributedCacheEntryOptions options)
        {
            file.CreationTime = options.AbsoluteExpiration.HasValue
                ? options.AbsoluteExpiration.Value.LocalDateTime
                : DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration ?? TimeSpan.FromDays(1));
        }

        private static int ScanDirectory(DirectoryInfo directory)
        {
            var deletedFileCount = 0;

            try
            {
                deletedFileCount = directory.EnumerateDirectories().Sum(ScanDirectory);

                foreach (var file in directory.EnumerateFiles().Where(f => f.CreationTime <= DateTime.Now))
                {
                    file.Delete();
                    deletedFileCount++;
                }

                if (!directory.EnumerateFileSystemInfos().Any())
                {
                    directory.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed cleaning {directory.FullName}: {ex.Message}");
            }

            return deletedFileCount;
        }
    }
}
