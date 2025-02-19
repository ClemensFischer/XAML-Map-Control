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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on local image files.
    /// </summary>
    public sealed class ImageFileCache : IDistributedCache, IDisposable
    {
        private readonly MemoryDistributedCache memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        private readonly StorageFolder rootFolder;
        private readonly Timer cleanTimer;
        private bool cleaning;

        public ImageFileCache(StorageFolder folder)
            : this(folder, TimeSpan.FromHours(1))
        {
        }

        public ImageFileCache(StorageFolder folder, TimeSpan autoCleanInterval)
        {
            rootFolder = folder ?? throw new ArgumentException($"The {nameof(folder)} argument must not be null or empty.", nameof(folder));

            Debug.WriteLine($"{nameof(ImageFileCache)}: {rootFolder.Path}");

            if (autoCleanInterval > TimeSpan.Zero)
            {
                cleanTimer = new Timer(_ => CleanAsync().Wait(), null, TimeSpan.Zero, autoCleanInterval);
            }
        }

        public void Dispose()
        {
            cleanTimer?.Dispose();
        }

        public byte[] Get(string key)
        {
            throw new NotSupportedException();
        }

        public void Set(string key, byte[] buffer, DistributedCacheEntryOptions options)
        {
            throw new NotSupportedException();
        }

        public void Remove(string key)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotSupportedException();
        }

        public void Refresh(string key)
        {
            throw new NotSupportedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotSupportedException();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var buffer = await memoryCache.GetAsync(key, token).ConfigureAwait(false);

            if (buffer == null)
            {
                try
                {
                    var item = await rootFolder.TryGetItemAsync(key.Replace('/', '\\'));

                    if (item is StorageFile file && file.DateCreated > DateTimeOffset.Now)
                    {
                        buffer = (await FileIO.ReadBufferAsync(file)).ToArray();

                        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = file.DateCreated };

                        await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed reading {key}: {ex.Message}");
                }
            }

            return buffer;
        }

        public async Task SetAsync(string key, byte[] buffer, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);

            if (buffer?.Length > 0)
            {
                try
                {
                    var keyComponents = key.Split('/');
                    var folder = rootFolder;

                    for (int i = 0; i < keyComponents.Length - 1; i++)
                    {
                        folder = await folder.CreateFolderAsync(keyComponents[i], CreationCollisionOption.OpenIfExists);
                    }

                    var file = await folder.CreateFileAsync(keyComponents[keyComponents.Length - 1], CreationCollisionOption.OpenIfExists);

                    await FileIO.WriteBytesAsync(file, buffer);

                    var expiration = options.AbsoluteExpiration.HasValue
                        ? options.AbsoluteExpiration.Value.LocalDateTime
                        : DateTime.Now.Add(options.AbsoluteExpirationRelativeToNow ?? (options.SlidingExpiration ?? TimeSpan.FromDays(1)));

                    File.SetCreationTime(file.Path, expiration);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(ImageFileCache)}: Failed writing {key}: {ex.Message}");
                }
            }
        }

        public async Task CleanAsync()
        {
            if (!cleaning)
            {
                cleaning = true;

                foreach (var folder in await rootFolder.GetFoldersAsync())
                {
                    var deletedFileCount = await CleanFolder(folder);

                    if (deletedFileCount > 0)
                    {
                        Debug.WriteLine($"{nameof(ImageFileCache)}: Deleted {deletedFileCount} expired files in {folder.Name}.");
                    }
                }

                cleaning = false;
            }
        }

        private static async Task<int> CleanFolder(StorageFolder folder)
        {
            var deletedFileCount = 0;

            try
            {
                foreach (var subFolder in await folder.GetFoldersAsync())
                {
                    deletedFileCount += await CleanFolder(subFolder);
                }

                foreach (var file in (await folder.GetFilesAsync()).Where(f => f.DateCreated <= DateTime.Now))
                {
                    await file.DeleteAsync();
                    deletedFileCount++;
                }

                if ((await folder.GetItemsAsync()).Count == 0)
                {
                    await folder.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageFileCache)}: Failed cleaning {folder.Path}: {ex.Message}");
            }

            return deletedFileCount;
        }
    }
}
