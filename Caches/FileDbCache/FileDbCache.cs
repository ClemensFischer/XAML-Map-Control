using FileDbNs;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on FileDb, https://github.com/eztools-software/FileDb.
    /// </summary>
    public sealed class FileDbCache : IDistributedCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb { AutoFlush = true };
        private readonly Timer timer;

        public FileDbCache(string path)
            : this(path, TimeSpan.FromHours(1))
        {
        }

        public FileDbCache(string path, TimeSpan expirationScanFrequency)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path ?? "", "TileCache.fdb");
            }

            try
            {
                fileDb.Open(path);

                Debug.WriteLine($"{nameof(FileDbCache)}: Opened database {path}");
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
                    new Field(keyField, DataTypeEnum.String) { IsPrimaryKey = true },
                    new Field(valueField, DataTypeEnum.Byte) { IsArray = true },
                    new Field(expiresField, DataTypeEnum.DateTime)
                });

                Debug.WriteLine($"{nameof(FileDbCache)}: Created database {path}");
            }

            if (expirationScanFrequency > TimeSpan.Zero)
            {
                timer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, expirationScanFrequency);
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
                    var record = fileDb.GetRecordByKey(key, new string[] { valueField, expiresField }, false);

                    if (record != null && (DateTime)record[1] > DateTime.UtcNow)
                    {
                        value = (byte[])record[0];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(FileDbCache)}.Get({key}): {ex.Message}");
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
                    { valueField, value },
                    { expiresField, expiration }
                };

                try
                {
                    if (fileDb.GetRecordByKey(key, new string[0], false) != null)
                    {
                        fileDb.UpdateRecordByKey(key, fieldValues);
                    }
                    else
                    {
                        fieldValues.Add(keyField, key);
                        fileDb.AddRecord(fieldValues);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(FileDbCache)}.Set({key}): {ex.Message}");
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
                    Debug.WriteLine($"{nameof(FileDbCache)}.Remove({key}): {ex.Message}");
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
            var deleted = fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThanOrEqual));

            if (deleted > 0)
            {
                fileDb.Clean();

                Debug.WriteLine($"{nameof(FileDbCache)}: Deleted {deleted} expired items");
            }
        }
    }
}
