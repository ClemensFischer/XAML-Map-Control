// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
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
    public class FileDbCache : IDistributedCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb { AutoFlush = true };

        public FileDbCache(string path)
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
                Debug.WriteLine($"FileDbCache: Opened database {path}");

                Clean();
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

                Debug.WriteLine($"FileDbCache: Created database {path}");
            }
        }

        public void Dispose()
        {
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
                Debug.WriteLine($"FileDbCache.Get({key}): {ex.Message}");
            }

            return value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckArguments(key, value, options);

            DateTime expiration;

            if (options.AbsoluteExpiration.HasValue)
            {
                expiration = options.AbsoluteExpiration.Value.DateTime;
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                expiration = DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.SlidingExpiration.HasValue)
            {
                expiration = DateTime.UtcNow.Add(options.SlidingExpiration.Value);
            }
            else
            {
                expiration = DateTime.UtcNow.Add(TimeSpan.FromDays(1));
            }

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
                Debug.WriteLine($"FileDbCache.Set({key}): {ex.Message}");
            }
        }

        public void Refresh(string key)
        {
            CheckArgument(key);
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
                Debug.WriteLine($"FileDbCache.Remove({key}): {ex.Message}");
            }
        }

        public void Clean()
        {
            var deleted = fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThanOrEqual));

            if (deleted > 0)
            {
                Debug.WriteLine($"FileDbCache: Deleted {deleted} expired items");
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

        public Task CleanAsync()
        {
            Clean();
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
