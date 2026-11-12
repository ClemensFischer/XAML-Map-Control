using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching;

public class ImageFileCacheOptions : IOptions<ImageFileCacheOptions>
{
    public ImageFileCacheOptions Value => this;

    public string Path { get; set; }

    public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromHours(1);
}

/// <summary>
/// IDistributedCache implementation that creates a single file per cache entry.
/// The cache expiration time is stored in the file's LastWriteTime property.
/// </summary>
public sealed partial class ImageFileCache : IDistributedCache, IDisposable
{
    private readonly DirectoryInfo rootDirectory;
    private readonly Timer timer;
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

        logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(typeof(ImageFileCache));
        logger.LogInformation("Started in {Path}", rootDirectory.FullName);

        if (options.ExpirationScanFrequency > TimeSpan.Zero)
        {
            timer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, options.ExpirationScanFrequency);
        }
    }

    public void Dispose()
    {
        timer?.Dispose();
    }

    public byte[] Get(string key)
    {
        byte[] value = null;

        if (!string.IsNullOrEmpty(key))
        {
            var file = GetFile(key);

            try
            {
                if (file != null && file.Exists && !file.IsExpired)
                {
                    value = file.ReadAllBytes();

                    logger.LogDebug("Read {Path}", file.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed reading {Path}", file.FullName);
            }
        }

        return value;
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        byte[] value = null;

        if (!string.IsNullOrEmpty(key))
        {
            var file = GetFile(key);

            try
            {
                if (file != null && file.Exists && !file.IsExpired && !token.IsCancellationRequested)
                {
                    value = await file.ReadAllBytesAsync(token).ConfigureAwait(false);

                    logger.LogDebug("Read {Path}", file.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed reading {Path}", file.FullName);
            }
        }

        return value;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        if (!string.IsNullOrEmpty(key) && value != null && options != null)
        {
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

                    file.SetExpiration(options);

                    logger.LogDebug("Wrote {Path}", file.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed writing {Path}", file.FullName);
            }
        }
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(key) && value != null && options != null)
        {
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

                    file.SetExpiration(options);

                    logger.LogDebug("Wrote {Path}", file.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed writing {Path}", file.FullName);
            }
        }
    }

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
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
                logger.LogError(ex, "Failed deleting {Path}", file.FullName);
            }
        }
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(key))
        {
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
                logger.LogError(ex, "Failed deleting {Path}", file.FullName);
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
                    logger.LogInformation("Deleted {Count} expired items in {Path}", deletedFileCount, directory.FullName);
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

            foreach (var file in directory.EnumerateFiles().Where(file => file.IsExpired))
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
            logger.LogError(ex, "Failed cleaning {Path}", directory.FullName);
        }

        return deletedFileCount;
    }

    private FileInfo GetFile(string key)
    {
        try
        {
            return new FileInfo(Path.Combine(rootDirectory.FullName, Path.Combine(key.Split('/'))));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid key {Key}", key);
        }

        return null;
    }
}

internal static class FileInfoExtension
{
    extension(FileInfo file)
    {
        public bool IsExpired => file.LastWriteTime <= DateTime.Now;

        public DistributedCacheEntryOptions CacheOptions => new() { AbsoluteExpiration = file.LastWriteTime };

        public void SetExpiration(DistributedCacheEntryOptions options)
        {
            file.LastWriteTime = options.AbsoluteExpiration.HasValue
                ? options.AbsoluteExpiration.Value.LocalDateTime
                : DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration ?? TimeSpan.FromDays(1));
        }

        public byte[] ReadAllBytes()
        {
            using var stream = file.OpenRead();
            var buffer = new byte[stream.Length];
            var offset = 0;

            while (offset < buffer.Length)
            {
                offset += stream.Read(buffer, offset, buffer.Length - offset);
            }

            return buffer;
        }

        public async Task<byte[]> ReadAllBytesAsync(CancellationToken token)
        {
            using var stream = file.OpenRead();
            var buffer = new byte[stream.Length];
            var offset = 0;

            while (offset < buffer.Length)
            {
                offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
            }

            return buffer;
        }
    }
}
