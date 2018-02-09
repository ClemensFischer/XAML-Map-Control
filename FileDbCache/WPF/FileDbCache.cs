// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using FileDbNs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace MapControl.Caching
{
    /// <summary>
    /// ObjectCache implementation based on FileDb, a free and simple No-SQL database by EzTools Software.
    /// See http://www.eztools-software.com/tools/filedb/.
    /// </summary>
    public sealed class FileDbCache : ObjectCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb();
        private readonly string dbPath;

        public FileDbCache(string path, bool autoFlush = true, int autoCleanThreshold = -1)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The parameter path must not be null or empty.");
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path, "TileCache.fdb");
            }

            dbPath = path;

            fileDb.AutoFlush = autoFlush;
            fileDb.AutoCleanThreshold = autoCleanThreshold;

            try
            {
                fileDb.Open(dbPath, false);

                Debug.WriteLine("FileDbCache: Opened database with {0} cached items in {1}", fileDb.NumRecords, dbPath);

                Clean();
            }
            catch
            {
                CreateDatabase();
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
        }

        public override string Name
        {
            get { return string.Empty; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations; }
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, null); }
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException("FileDbCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("FileDbCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            if (fileDb.IsOpen)
            {
                try
                {
                    return fileDb.NumRecords;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.NumRecords: " + ex.Message);
                }
            }

            return 0;
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            if (fileDb.IsOpen)
            {
                try
                {
                    return fileDb.GetRecordByKey(key, new string[0], false) != null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\"): {1}", key, ex.Message);
                }
            }

            return false;
        }

        public override object Get(string key, string regionName = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            if (fileDb.IsOpen)
            {
                try
                {
                    var record = fileDb.GetRecordByKey(key, new string[] { valueField }, false);

                    if (record != null)
                    {
                        return record[0];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\"): {1}", key, ex.Message);
                }
            }

            return null;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var value = Get(key, regionName);

            return value != null ? new CacheItem(key, value) : null;
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            return keys.ToDictionary(key => key, key => Get(key, regionName));
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (value == null)
            {
                throw new ArgumentNullException("The parameter value must not be null.");
            }

            if (policy == null)
            {
                throw new ArgumentNullException("The parameter policy must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            if (fileDb.IsOpen)
            {
                var expiration = DateTime.MaxValue;

                if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration)
                {
                    expiration = policy.AbsoluteExpiration.DateTime;
                }
                else if (policy.SlidingExpiration != NoSlidingExpiration)
                {
                    expiration = DateTime.UtcNow + policy.SlidingExpiration;
                }

                if (!AddOrUpdateRecord(key, value, expiration) && RepairDatabase())
                {
                    AddOrUpdateRecord(key, value, expiration);
                }
            }
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var oldValue = Get(key, regionName);

            Set(key, value, policy);

            return oldValue;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return AddOrGetExisting(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
        {
            var oldItem = GetCacheItem(item.Key, item.RegionName);

            Set(item, policy);

            return oldItem;
        }

        public override object Remove(string key, string regionName = null)
        {
            var oldValue = Get(key, regionName);

            if (oldValue != null)
            {
                try
                {
                    fileDb.DeleteRecordByKey(key);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.DeleteRecordByKey(\"{0}\"): {1}", key, ex.Message);
                }
            }

            return oldValue;
        }

        public void Dispose()
        {
            Close();
        }

        public void Flush()
        {
            if (fileDb.IsOpen)
            {
                fileDb.Flush();
            }
        }

        public void Clean()
        {
            if (fileDb.IsOpen)
            {
                int deleted = fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThan));

                if (deleted > 0)
                {
                    Debug.WriteLine("FileDbCache: Deleted {0} expired items", deleted);
                    fileDb.Clean();
                }
            }
        }

        private void Close()
        {
            if (fileDb.IsOpen)
            {
                fileDb.Close();
            }
        }

        private void CreateDatabase()
        {
            Close();

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            }

            fileDb.Create(dbPath, new Field[]
            {
                new Field(keyField, DataTypeEnum.String) { IsPrimaryKey = true },
                new Field(valueField, DataTypeEnum.Byte) { IsArray = true },
                new Field(expiresField, DataTypeEnum.DateTime)
            });

            Debug.WriteLine("FileDbCache: Created database " + dbPath);
        }

        private bool RepairDatabase()
        {
            try
            {
                fileDb.Reindex();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDbCache: FileDb.Reindex(): " + ex.Message);
            }

            try
            {
                CreateDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDbCache: Failed creating database {0}: {1}", dbPath, ex.Message);
            }

            return false;
        }

        private bool AddOrUpdateRecord(string key, object value, DateTime expiration)
        {
            var fieldValues = new FieldValues(3);
            fieldValues.Add(valueField, value);
            fieldValues.Add(expiresField, expiration);

            bool recordExists;

            try
            {
                recordExists = fileDb.GetRecordByKey(key, new string[0], false) != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\"): {1}", key, ex.Message);
                return false;
            }

            if (recordExists)
            {
                try
                {
                    fileDb.UpdateRecordByKey(key, fieldValues);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.UpdateRecordByKey(\"{0}\"): {1}", key, ex.Message);
                    return false;
                }
            }
            else
            {
                try
                {
                    fieldValues.Add(keyField, key);
                    fileDb.AddRecord(fieldValues);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.AddRecord(\"{0}\"): {1}", key, ex.Message);
                    return false;
                }
            }

            //Debug.WriteLine("FileDbCache: Writing \"{0}\", Expires {1}", key, expiration.ToLocalTime());
            return true;
        }
    }
}
