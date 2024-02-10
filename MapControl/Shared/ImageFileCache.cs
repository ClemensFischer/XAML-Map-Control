// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
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
        private static readonly byte[] expirationTagBytes = Encoding.ASCII.GetBytes("EXPIRES:");
        private static readonly long expirationTagLong = BitConverter.ToInt64(expirationTagBytes, 0);

        private readonly IDistributedCache memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        private readonly string rootDirectory;

        public ImageFileCache(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException($"The {nameof(directory)} argument must not be null or empty.", nameof(directory));
            }

            rootDirectory = directory;

            Debug.WriteLine($"Created ImageFileCache in {rootDirectory}");

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

                        var expiration = ReadExpiration(ref buffer);

                        if (expiration.HasValue)
                        {
                            if (expiration.Value > DateTimeOffset.UtcNow)
                            {
                                var options = new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpiration = expiration
                                };

                                memoryCache.Set(key, buffer, options);
                            }
                            else
                            {
                                File.Delete(path);
                                buffer = null;
                            }
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

                        var expiration = ReadExpiration(ref buffer);

                        if (expiration.HasValue)
                        {
                            if (expiration.Value > DateTimeOffset.UtcNow)
                            {
                                var options = new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpiration = expiration
                                };

                                await memoryCache.SetAsync(key, buffer, options, token).ConfigureAwait(false);
                            }
                            else
                            {
                                File.Delete(path);
                                buffer = null;
                            }
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

                        var expiration = GetExpiration(options);

                        if (expiration.HasValue)
                        {
                            var expirationValueBytes = BitConverter.GetBytes(expiration.Value.Ticks);

                            Write(stream, expirationTagBytes);
                            Write(stream, expirationValueBytes);
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

                        var expiration = GetExpiration(options);

                        if (expiration.HasValue)
                        {
                            var expirationValueBytes = BitConverter.GetBytes(expiration.Value.Ticks);

                            await WriteAsync(stream, expirationTagBytes).ConfigureAwait(false);
                            await WriteAsync(stream, expirationValueBytes).ConfigureAwait(false);
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
                Debug.WriteLine($"ImageFileCache: Invalid key {rootDirectory}/{key}: {ex.Message}");
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
                BitConverter.ToInt64(buffer, buffer.Length - 16) == expirationTagLong)
            {
                var expirationTicks = BitConverter.ToInt64(buffer, buffer.Length - 8);

                expiration = new DateTimeOffset(expirationTicks, TimeSpan.Zero);
            }

            return expiration;
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
#if !UWP
        static partial void SetAccessControl(string path)
        {
            var fileInfo = new FileInfo(path);
            var fileSecurity = fileInfo.GetAccessControl();
            var fullControlRule = new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                FileSystemRights.FullControl, AccessControlType.Allow);

            fileSecurity.AddAccessRule(fullControlRule);
            fileInfo.SetAccessControl(fileSecurity);
        }
#endif
    }
}
