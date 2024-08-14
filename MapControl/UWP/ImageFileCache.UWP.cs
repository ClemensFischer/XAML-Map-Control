// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on local image files.
    /// </summary>
    public partial class ImageFileCache : IDistributedCache
    {
        private static readonly byte[] expirationTag = Encoding.ASCII.GetBytes("EXPIRES:");

        private readonly MemoryDistributedCache memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        private readonly StorageFolder rootFolder;

        public ImageFileCache(StorageFolder folder)
        {
            rootFolder = folder ?? throw new ArgumentException($"The {nameof(folder)} argument must not be null or empty.", nameof(folder));

            Debug.WriteLine($"ImageFileCache: {rootFolder.Path}");

            _ = Task.Factory.StartNew(CleanAsync, TaskCreationOptions.LongRunning);
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

                    if (item is StorageFile file)
                    {
                        buffer = (await FileIO.ReadBufferAsync(file)).ToArray();

                        if (CheckExpiration(ref buffer, out DistributedCacheEntryOptions options))
                        {
                            await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed reading {key}: {ex.Message}");
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

                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await stream.WriteAsync(buffer.AsBuffer());

                        if (GetExpirationBytes(options, out byte[] expiration))
                        {
                            await stream.WriteAsync(expiration.AsBuffer());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {key}: {ex.Message}");
                }
            }
        }

        public async Task CleanAsync()
        {
            var deletedFileCount = await CleanFolder(rootFolder);

            if (deletedFileCount > 0)
            {
                Debug.WriteLine($"ImageFileCache: Deleted {deletedFileCount} expired files.");
            }
        }

        private static async Task<int> CleanFolder(StorageFolder folder)
        {
            var deletedFileCount = 0;

            try
            {
                foreach (var f in await folder.GetFoldersAsync())
                {
                    deletedFileCount += await CleanFolder(f);
                }

                foreach (var f in await folder.GetFilesAsync())
                {
                    deletedFileCount += await CleanFile(f);
                }

                if ((await folder.GetItemsAsync()).Count == 0)
                {
                    await folder.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed cleaning {folder.Path}: {ex.Message}");
            }

            return deletedFileCount;
        }

        private static async Task<int> CleanFile(StorageFile file)
        {
            var deletedFileCount = 0;
            var size = (await file.GetBasicPropertiesAsync()).Size;

            if (size > 16)
            {
                try
                {
                    var hasExpired = false;

                    using (var stream = await file.OpenReadAsync())
                    {
                        stream.Seek(size - 16);

                        var buffer = await stream.ReadAsync(new Buffer(16), 16, InputStreamOptions.None);

                        hasExpired = buffer.Length == 16
                            && GetExpirationTicks(buffer.ToArray(), out long expiration)
                            && expiration <= DateTimeOffset.UtcNow.Ticks;
                    }

                    if (hasExpired)
                    {
                        await file.DeleteAsync();
                        deletedFileCount = 1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed cleaning {file.Path}: {ex.Message}");
                }
            }

            return deletedFileCount;
        }

        private static bool CheckExpiration(ref byte[] buffer, out DistributedCacheEntryOptions options)
        {
            if (GetExpirationTicks(buffer, out long expiration))
            {
                if (expiration > DateTimeOffset.UtcNow.Ticks)
                {
                    Array.Resize(ref buffer, buffer.Length - 16);

                    options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = new DateTimeOffset(expiration, TimeSpan.Zero)
                    };

                    return true;
                }

                buffer = null; // buffer has expired
            }

            options = null;
            return false;
        }

        private static bool GetExpirationTicks(byte[] buffer, out long expirationTicks)
        {
            if (buffer.Length >= 16 &&
                expirationTag.SequenceEqual(buffer.Skip(buffer.Length - 16).Take(8)))
            {
                expirationTicks = BitConverter.ToInt64(buffer, buffer.Length - 8);
                return true;
            }

            expirationTicks = 0;
            return false;
        }

        private static bool GetExpirationBytes(DistributedCacheEntryOptions options, out byte[] expirationBytes)
        {
            long expirationTicks;

            if (options.AbsoluteExpiration.HasValue)
            {
                expirationTicks = options.AbsoluteExpiration.Value.Ticks;
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                expirationTicks = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value).Ticks;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                expirationTicks = DateTimeOffset.UtcNow.Add(options.SlidingExpiration.Value).Ticks;
            }
            else
            {
                expirationBytes = null;
                return false;
            }

            expirationBytes = expirationTag.Concat(BitConverter.GetBytes(expirationTicks)).ToArray();
            return true;
        }
    }
}
