using FileDbNs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public class FileDbCacheOptions : IOptions<FileDbCacheOptions>
    {
        public FileDbCacheOptions Value => this;

        public string Path { get; set; }

        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// IDistributedCache implementation based on FileDb, https://github.com/eztools-software/FileDb.
    /// </summary>
    public sealed class FileDbCache : IDistributedCache, IDisposable
    {
        private const string KeyField = "Key";
        private const string ValueField = "Value";
        private const string ExpiresField = "Expires";

        private readonly FileDb fileDb = new FileDb { AutoFlush = true };
        private readonly Timer timer;
        private readonly ILogger logger;

        public FileDbCache(string path, ILoggerFactory loggerFactory = null)
            : this(new FileDbCacheOptions { Path = path }, loggerFactory)
        {
        }

        public FileDbCache(IOptions<FileDbCacheOptions> optionsAccessor, ILoggerFactory loggerFactory = null)
            : this(optionsAccessor.Value, loggerFactory)
        {
        }

        public FileDbCache(FileDbCacheOptions options, ILoggerFactory loggerFactory = null)
        {
            var path = options.Path;

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path ?? "", "TileCache.fdb");
            }

            logger = loggerFactory?.CreateLogger<FileDbCache>();

            try
            {
                fileDb.Open(path);

                logger?.LogInformation("Opened database {path}", path);
            }
            catch
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                fileDb.Create(path, new Field[]
                {
                    new Field(KeyField, DataTypeEnum.String) { IsPrimaryKey = true },
                    new Field(ValueField, DataTypeEnum.Byte) { IsArray = true },
                    new Field(ExpiresField, DataTypeEnum.DateTime)
                });

                logger?.LogInformation("Created database {path}", path);
            }

            if (options.ExpirationScanFrequency > TimeSpan.Zero)
            {
                timer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, options.ExpirationScanFrequency);
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
            fileDb.Dispose();
        }

        public byte[] Get(string key)
        {
            byte[] value = null;

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    var record = fileDb.GetRecordByKey(key, new string[] { ValueField, ExpiresField }, false);

                    if (record != null && (DateTime)record[1] > DateTime.UtcNow)
                    {
                        value = (byte[])record[0];
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Get({key})", key);
                }
            }

            return value;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (!string.IsNullOrEmpty(key) && value != null && options != null)
            {
                var expiration = options.AbsoluteExpiration.HasValue
                    ? options.AbsoluteExpiration.Value.UtcDateTime
                    : DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration ?? TimeSpan.FromDays(1));

                var fieldValues = new FieldValues(3)
                {
                    { ValueField, value },
                    { ExpiresField, expiration }
                };

                try
                {
                    if (fileDb.GetRecordByKey(key, new string[0], false) != null)
                    {
                        fileDb.UpdateRecordByKey(key, fieldValues);
                    }
                    else
                    {
                        fieldValues.Add(KeyField, key);
                        fileDb.AddRecord(fieldValues);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Set({key})", key);
                }
            }
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);

            return Task.CompletedTask;
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
                try
                {
                    fileDb.DeleteRecordByKey(key);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Remove({key})", key);
                }
            }
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);

            return Task.CompletedTask;
        }

        public void DeleteExpiredItems()
        {
            var deletedItemsCount = fileDb.DeleteRecords(new FilterExpression(ExpiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThanOrEqual));

            if (deletedItemsCount > 0)
            {
                fileDb.Clean();

                logger?.LogInformation("Deleted {count} expired items", deletedItemsCount);
            }
        }
    }
}
