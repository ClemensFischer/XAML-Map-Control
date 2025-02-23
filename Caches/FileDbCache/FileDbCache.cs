// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
    /// IDistributedCache implementation based on FileDb, a free and simple No-SQL database by EzTools Software.
    /// See http://www.eztools-software.com/tools/filedb/.
    /// </summary>
    public sealed class FileDbCache : IDistributedCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb { AutoFlush = true };
        private readonly Timer expirationScanTimer;

        public FileDbCache(string path)
            : this(path, TimeSpan.FromHours(1))
        {
        }

        public FileDbCache(string path, TimeSpan expirationScanFrequency)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The {nameof(path)} argument must not be null or empty.", nameof(path));
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path, "TileCache.fdb");
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
                expirationScanTimer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, expirationScanFrequency);
            }
        }

        public void Dispose()
        {
            expirationScanTimer?.Dispose();
            fileDb.Dispose();
        }

        public byte[] Get(string key)
        {
            CheckArgument(key);

            byte[] value = null;

            try
            {
                var record = fileDb.GetRecordByKey(key, new string[] { valueField, expiresField }, false);

                if (record != null)
                {
                    if ((DateTime)record[1] > DateTime.UtcNow)
                    {
                        value = (byte[])record[0];
                    }
                    else
                    {
                        fileDb.DeleteRecordByKey(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(FileDbCache)}.Get({key}): {ex.Message}");
            }

            return value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckArguments(key, value, options);

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

        public void Refresh(string key)
        {
        }

        public void Remove(string key)
        {
            CheckArgument(key);

            try
            {
                fileDb.DeleteRecordByKey(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(FileDbCache)}.Remove({key}): {ex.Message}");
            }
        }

        public void DeleteExpiredItems()
        {
            var deleted = fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThanOrEqual));

            if (deleted > 0)
            {
                Debug.WriteLine($"{nameof(FileDbCache)}: Deleted {deleted} expired items");
                fileDb.Clean();
            }
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);

            return Task.CompletedTask;
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            Refresh(key);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);

            return Task.CompletedTask;
        }

        private static void CheckArgument(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"The {nameof(key)} argument must not be null or empty.", nameof(key));
            }
        }

        private static void CheckArguments(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckArgument(key);

            if (value == null)
            {
                throw new ArgumentNullException($"The {nameof(value)} argument must not be null.", nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException($"The {nameof(options)} argument must not be null.", nameof(options));
            }
        }
    }
}
