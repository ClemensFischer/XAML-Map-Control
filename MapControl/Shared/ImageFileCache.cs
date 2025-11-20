using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public class ImageFileCacheOptions : IOptions<ImageFileCacheOptions>
    {
        public ImageFileCacheOptions Value => this;

        public string Path { get; set; }

        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// IDistributedCache implementation that creates a single file per cache entry.
    /// The cache expiration time is stored in the file's CreationTime property.
    /// </summary>
    public sealed class ImageFileCache : IDistributedCache, IDisposable
    {
        private readonly MemoryDistributedCache memoryCache;
        private readonly DirectoryInfo rootDirectory;
        private readonly Timer expirationScanTimer;
        private readonly ILogger logger;
        private bool scanningExpiration;

        public ImageFileCache(string path, ILoggerFactory loggerFactory = null)
            : this(new ImageFileCacheOptions { Path = path }, loggerFactory)
        {
        }

        public ImageFileCache(IOptions<ImageFileCacheOptions> optionsAccessor, ILoggerFactory loggerFactory = null)
            : this(optionsAccessor.Value, loggerFactory)
        {
        }

        public ImageFileCache(ImageFileCacheOptions options, ILoggerFactory loggerFactory = null)
        {
            var path = options.Path;

            rootDirectory = new DirectoryInfo(!string.IsNullOrEmpty(path) ? path : "TileCache");
            rootDirectory.Create();

            logger = loggerFactory?.CreateLogger(typeof(ImageFileCache));
            logger?.LogInformation("Started in {name}", rootDirectory.FullName);

            var memoryCacheOptions = new MemoryDistributedCacheOptions();

            if (options.ExpirationScanFrequency > TimeSpan.Zero)
            {
                memoryCacheOptions.ExpirationScanFrequency = options.ExpirationScanFrequency;

                expirationScanTimer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, options.ExpirationScanFrequency);
            }

            memoryCache = new MemoryDistributedCache(Options.Create(memoryCacheOptions));
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

                            logger?.LogDebug("Read {name}", file.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed reading {name}", file.FullName);
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

                            logger?.LogDebug("Read {name}", file.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed reading {name}", file.FullName);
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

                        logger?.LogDebug("Wrote {name}", file.FullName);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed writing {name}", file.FullName);
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

                        logger?.LogDebug("Wrote {name}", file.FullName);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed writing {name}", file.FullName);
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
                    logger?.LogError(ex, "Failed deleting {name}", file.FullName);
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
                    logger?.LogError(ex, "Failed deleting {name}", file.FullName);
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
                        logger?.LogInformation("Deleted {count} expired items in {name}", deletedFileCount, directory.FullName);
                    }
                }

                scanningExpiration = false;
            }
        }

        private int ScanDirectory(DirectoryInfo directory)
        {
            var deletedFileCount = 0;

            try
            {
                deletedFileCount = directory.EnumerateDirectories().Sum(ScanDirectory);

                foreach (var file in directory.EnumerateFiles()
                    .Where(file => file.CreationTime > file.LastWriteTime &&
                                   file.CreationTime <= DateTime.Now))
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
                logger?.LogError(ex, "Failed cleaning {name}", directory.FullName);
            }

            return deletedFileCount;
        }

        private FileInfo GetFile(string key)
        {
            FileInfo file = null;

            try
            {
                file = new FileInfo(Path.Combine(rootDirectory.FullName, Path.Combine(key.Split('/'))));
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Invalid key {key}", key);
            }

            return file;
        }

        private static byte[] ReadAllBytes(FileInfo file)
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[stream.Length];
            var offset = 0;

            while (offset < buffer.Length)
            {
                offset += stream.Read(buffer, offset, buffer.Length - offset);
            }

            return buffer;
        }

        private static async Task<byte[]> ReadAllBytes(FileInfo file, CancellationToken token)
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[stream.Length];
            var offset = 0;

            while (offset < buffer.Length)
            {
                offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
            }

            return buffer;
        }

        private static void SetExpiration(FileInfo file, DistributedCacheEntryOptions options)
        {
            file.CreationTime = options.AbsoluteExpiration.HasValue
                ? options.AbsoluteExpiration.Value.LocalDateTime
                : DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration ?? TimeSpan.FromDays(1));
        }
    }
}
