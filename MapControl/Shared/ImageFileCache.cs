// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on local image files.
    /// </summary>
    public partial class ImageFileCache : IDistributedCache
    {
        private static readonly byte[] expirationTag = Encoding.ASCII.GetBytes("EXPIRES:");

        private readonly MemoryDistributedCache memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        private readonly string rootDirectory;

        public ImageFileCache(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException($"The {nameof(directory)} argument must not be null or empty.", nameof(directory));
            }

            rootDirectory = directory;

            Debug.WriteLine($"ImageFileCache: {rootDirectory}");

            ThreadPool.QueueUserWorkItem(o => Clean());
        }

        public byte[] Get(string key)
        {
            var buffer = memoryCache.Get(key);

            if (buffer == null)
            {
                var path = GetPath(key);

                try
                {
                    if (path != null && File.Exists(path))
                    {
                        buffer = File.ReadAllBytes(path);

                        if (CheckExpiration(ref buffer, out DistributedCacheEntryOptions options))
                        {
                            memoryCache.Set(key, buffer, options);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
                }
            }

            return buffer;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var buffer = await memoryCache.GetAsync(key, token).ConfigureAwait(false);

            if (buffer == null)
            {
                var path = GetPath(key);

                try
                {
                    if (path != null && File.Exists(path) && !token.IsCancellationRequested)
                    {
                        buffer = await ReadAllBytesAsync(path).ConfigureAwait(false);

                        if (CheckExpiration(ref buffer, out DistributedCacheEntryOptions options))
                        {
                            await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
                }
            }

            return buffer;
        }

        public void Set(string key, byte[] buffer, DistributedCacheEntryOptions options)
        {
            memoryCache.Set(key, buffer, options);

            var path = GetPath(key);

            if (path != null && buffer?.Length > 0)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        Write(stream, buffer);

                        if (GetExpirationBytes(options, out byte[] expiration))
                        {
                            Write(stream, expiration);
                        }
                    }

                    SetAccessControl(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
            }
        }

        public async Task SetAsync(string key, byte[] buffer, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);

            var path = GetPath(key);

            if (path != null && buffer?.Length > 0 && !token.IsCancellationRequested)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        await WriteAsync(stream, buffer).ConfigureAwait(false);

                        if (GetExpirationBytes(options, out byte[] expiration))
                        {
                            await WriteAsync(stream, expiration).ConfigureAwait(false);
                        }
                    }

                    SetAccessControl(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
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

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await memoryCache.RemoveAsync(key, token);

            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path) && !token.IsCancellationRequested)
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed deleting {path}: {ex.Message}");
            }
        }

        public void Clean()
        {
            var deletedFileCount = CleanDirectory(new DirectoryInfo(rootDirectory));

            if (deletedFileCount > 0)
            {
                Debug.WriteLine($"ImageFileCache: Deleted {deletedFileCount} expired files.");
            }
        }

        public Task CleanAsync()
        {
            return Task.Factory.StartNew(Clean, TaskCreationOptions.LongRunning);
        }

        private string GetPath(string key)
        {
            try
            {
                return Path.Combine(rootDirectory, Path.Combine(key.Split('/')));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Invalid key {key}: {ex.Message}");
            }

            return null;
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

            if (file.Length > 16)
            {
                try
                {
                    var hasExpired = false;
                    var buffer = new byte[16];

                    using (var stream = file.OpenRead())
                    {
                        stream.Seek(-16, SeekOrigin.End);
                        hasExpired = stream.Read(buffer, 0, 16) == 16
                            && GetExpirationTicks(buffer, out long expiration)
                            && expiration <= DateTimeOffset.UtcNow.Ticks;
                    }

                    if (hasExpired)
                    {
                        file.Delete();
                        deletedFileCount = 1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed cleaning {file.FullName}: {ex.Message}");
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

#if NETFRAMEWORK
        private static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var buffer = new byte[stream.Length];
                var offset = 0;
                while (offset < buffer.Length)
                {
                    offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);
                }
                return buffer;
            }
        }
#else
        private static Task<byte[]> ReadAllBytesAsync(string path) => File.ReadAllBytesAsync(path);
#endif
        private static void Write(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

        private static Task WriteAsync(Stream stream, byte[] bytes) => stream.WriteAsync(bytes, 0, bytes.Length);

        static partial void SetAccessControl(string path);
#if !UWP && !AVALONIA
        static partial void SetAccessControl(string path)
        {
            var fileInfo = new FileInfo(path);
            var fileSecurity = fileInfo.GetAccessControl();
            var fullControlRule = new System.Security.AccessControl.FileSystemAccessRule(
                new System.Security.Principal.SecurityIdentifier(
                    System.Security.Principal.WellKnownSidType.BuiltinUsersSid, null),
                System.Security.AccessControl.FileSystemRights.FullControl,
                System.Security.AccessControl.AccessControlType.Allow);

            fileSecurity.AddAccessRule(fullControlRule);
            fileInfo.SetAccessControl(fileSecurity);
        }
#endif
    }
}
