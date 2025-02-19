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
        private readonly MemoryDistributedCache memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        private readonly DirectoryInfo rootDirectory;
        private readonly Timer cleanTimer;
        private bool cleaning;

        public ImageFileCache(string path)
            : this(path, TimeSpan.FromHours(1))
        {
        }

        public ImageFileCache(string path, TimeSpan autoCleanInterval)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The {nameof(path)} argument must not be null or empty.", nameof(path));
            }

            rootDirectory = new DirectoryInfo(path);

            Debug.WriteLine($"{nameof(ImageFileCache)}: {rootDirectory.FullName}");

            if (autoCleanInterval > TimeSpan.Zero)
            {
                cleanTimer = new Timer(_ => Clean(), null, TimeSpan.Zero, autoCleanInterval);
            }
        }

        public void Dispose()
        {
            cleanTimer?.Dispose();
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
                        using (var stream = file.OpenRead())
                        {
                            buffer = new byte[stream.Length];
                            var offset = 0;
                            while (offset < buffer.Length)
                            {
                                offset += stream.Read(buffer, offset, buffer.Length - offset);
                            }
                        }

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
                    if (file != null && file.Exists && file.CreationTime > DateTime.Now)
                    {
                        using (var stream = file.OpenRead())
                        {
                            buffer = new byte[stream.Length];
                            var offset = 0;
                            while (offset < buffer.Length)
                            {
                                offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
                            }
                        }

                        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.CreationTime };

                        await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
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
                    using (var stream = CreateFile(file, options))
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
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
                    using (var stream = CreateFile(file, options))
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    }
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

        public void Clean()
        {
            if (!cleaning)
            {
                cleaning = true;

                foreach (var directory in rootDirectory.EnumerateDirectories())
                {
                    var deletedFileCount = CleanDirectory(directory);

                    if (deletedFileCount > 0)
                    {
                        Debug.WriteLine($"{nameof(ImageFileCache)}: Deleted {deletedFileCount} expired files in {directory.Name}.");
                    }
                }

                cleaning = false;
            }
        }

        public Task CleanAsync()
        {
            return Task.Run(Clean);
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

        private static FileStream CreateFile(FileInfo file, DistributedCacheEntryOptions options)
        {
            file.Directory.Create();

            var stream = file.Create();

            try
            {
                file.CreationTime = options.AbsoluteExpiration.HasValue
                        ? options.AbsoluteExpiration.Value.LocalDateTime
                        : DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? (options.SlidingExpiration ?? TimeSpan.FromDays(1)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed setting creation time of {file.FullName}: {ex.Message}");
            }

            return stream;
        }

        private static int CleanDirectory(DirectoryInfo directory)
        {
            var deletedFileCount = 0;

            try
            {
                deletedFileCount = directory.EnumerateDirectories().Sum(CleanDirectory);

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
