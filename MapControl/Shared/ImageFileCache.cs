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
    /// IDistributedCache implementation based on local files.
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
            rootDirectory = new DirectoryInfo(!string.IsNullOrEmpty(path) ? path : "TileCache");
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
            byte[] value = null;

            if (!string.IsNullOrEmpty(key))
            {
                value = memoryCache.Get(key);

                if (value == null)
                {
                    var file = GetFile(key);

                    try
                    {
                        if (file != null && file.Exists && file.CreationTime > DateTime.Now)
                        {
                            value = ReadAllBytes(file);

                            var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.CreationTime };

                            memoryCache.Set(key, value, options);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{nameof(ImageFileCache)}: Failed reading {file.FullName}: {ex.Message}");
                    }
                }
            }

            return value;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            byte[] value = null;

            if (!string.IsNullOrEmpty(key))
            {
                value = await memoryCache.GetAsync(key, token).ConfigureAwait(false);

                if (value == null)
                {
                    var file = GetFile(key);

                    try
                    {
                        if (file != null && file.Exists && file.CreationTime > DateTime.Now && !token.IsCancellationRequested)
                        {
                            value = await ReadAllBytes(file, token).ConfigureAwait(false);

                            var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.CreationTime };

                            await memoryCache.SetAsync(key, value, options, token).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{nameof(ImageFileCache)}: Failed reading {file.FullName}: {ex.Message}");
                    }
                }
            }

            return value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (!string.IsNullOrEmpty(key) && value != null && options != null)
            {
                memoryCache.Set(key, value, options);

                var file = GetFile(key);

                try
                {
                    if (file != null && value?.Length > 0)
                    {
                        file.Directory.Create();

                        using (var stream = file.Create())
                        {
                            stream.Write(value, 0, value.Length);
                        }

                        SetExpiration(file, options);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed writing {file.FullName}: {ex.Message}");
                }
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(key) && value != null && options != null)
            {
                await memoryCache.SetAsync(key, value, options, token).ConfigureAwait(false);

                var file = GetFile(key);

                try
                {
                    if (file != null && value?.Length > 0 && !token.IsCancellationRequested)
                    {
                        file.Directory.Create();

                        using (var stream = file.Create())
                        {
                            await stream.WriteAsync(value, 0, value.Length, token).ConfigureAwait(false);
                        }

                        SetExpiration(file, options);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed writing {file.FullName}: {ex.Message}");
                }
            }
        }

        public void Refresh(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                memoryCache.Refresh(key);
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(key))
            {
                await memoryCache.RefreshAsync(key, token);
            }
        }

        public void Remove(string key)
        {
            if (!string.IsNullOrEmpty(key))
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
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(key))
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
