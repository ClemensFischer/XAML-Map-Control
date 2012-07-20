using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using FileDbNs;

namespace Caching
{
    /// <summary>
    /// ObjectCache implementation based on EzTools FileDb - http://www.eztools-software.com/tools/filedb/.
    /// </summary>
    public class FileDbCache : ObjectCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly BinaryFormatter formatter = new BinaryFormatter();
        private readonly FileDb fileDb = new FileDb { AutoFlush = false, AutoCleanThreshold = -1 };
        private readonly string name;
        private readonly string path;

        public FileDbCache(string name, NameValueCollection config)
            : this(name, config["directory"])
        {
            string autoFlush = config["autoFlush"];
            string autoCleanThreshold = config["autoCleanThreshold"];

            if (autoFlush != null)
            {
                try
                {
                    fileDb.AutoFlush = bool.Parse(autoFlush);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("The configuration parameter autoFlush must be a boolean value.", ex);
                }
            }

            if (autoCleanThreshold != null)
            {
                try
                {
                    fileDb.AutoCleanThreshold = int.Parse(autoCleanThreshold);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("The configuration parameter autoCleanThreshold must be an integer value.", ex);
                }
            }
        }

        public FileDbCache(string name, string directory)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The parameter name must not be null or empty.");
            }

            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The parameter directory must not be null or empty.");
            }

            this.name = name;
            path = Path.Combine(directory, name);

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path += ".fdb";
            }

            try
            {
                fileDb.Open(path, false);
                Trace.TraceInformation("FileDbCache: Opened database with {0} cached items in {1}", fileDb.NumRecords, path);
            }
            catch
            {
                CreateDatabase();
            }
        }

        public bool AutoFlush
        {
            get { return fileDb.AutoFlush; }
            set { fileDb.AutoFlush = value; }
        }

        public int AutoCleanThreshold
        {
            get { return fileDb.AutoCleanThreshold; }
            set { fileDb.AutoCleanThreshold = value; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.InMemoryProvider | DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations; }
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

            long count = 0;

            if (fileDb.IsOpen)
            {
                try
                {
                    count = fileDb.NumRecords;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("FileDbCache: FileDb.NumRecords failed: {0}", ex.Message);

                    if (RepairDatabase())
                    {
                        count = fileDb.NumRecords;
                    }
                }
            }

            return count;
        }

        private Record GetRecord(string key)
        {
            Record record = null;

            if (fileDb.IsOpen)
            {
                try
                {
                    record = fileDb.GetRecordByKey(key, new string[] { valueField }, false);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("FileDbCache: FileDb.GetRecordByKey(\"{0}\") failed: {1}", key, ex.Message);

                    if (RepairDatabase())
                    {
                        record = fileDb.GetRecordByKey(key, new string[] { valueField }, false);
                    }
                }
            }

            return record;
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

            return GetRecord(key) != null;
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

            object value = null;
            Record record = GetRecord(key);

            if (record != null)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream((byte[])record[0]))
                    {
                        value = formatter.Deserialize(stream);
                    }
                }
                catch (Exception ex1)
                {
                    Trace.TraceWarning("FileDbCache: Deserializing item \"{0}\" failed: {1}", key, ex1.Message);

                    try
                    {
                        fileDb.DeleteRecordByKey(key);
                    }
                    catch (Exception ex2)
                    {
                        Trace.TraceWarning("FileDbCache: FileDb.DeleteRecordByKey(\"{0}\") failed: {1}", key, ex2.Message);
                    }
                }
            }

            return value;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var value = Get(key, regionName);
            return value != null ? new CacheItem(key, value) : null;
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var values = new Dictionary<string, object>();

            foreach (string key in keys)
            {
                values[key] = Get(key);
            }

            return values;
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

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            if (fileDb.IsOpen)
            {
                byte[] valueBuffer = null;

                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, value);
                        valueBuffer = stream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("FileDbCache: Serializing item \"{0}\" failed: {1}", key, ex.Message);
                }

                if (valueBuffer != null)
                {
                    DateTime expires = DateTime.MaxValue;

                    if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration)
                    {
                        expires = policy.AbsoluteExpiration.DateTime;
                    }
                    else if (policy.SlidingExpiration != NoSlidingExpiration)
                    {
                        expires = DateTime.UtcNow + policy.SlidingExpiration;
                    }

                    try
                    {
                        AddOrUpdateRecord(key, valueBuffer, expires);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("FileDbCache: FileDb.UpdateRecordByKey(\"{0}\") failed: {1}", key, ex.Message);

                        if (RepairDatabase())
                        {
                            AddOrUpdateRecord(key, valueBuffer, expires);
                        }
                    }
                }
            }
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var oldValue = Get(key, regionName);
            Set(key, value, policy);
            return oldValue;
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
        {
            var oldItem = GetCacheItem(item.Key, item.RegionName);
            Set(item, policy);
            return oldItem;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return AddOrGetExisting(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
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
                    Trace.TraceWarning("FileDbCache: FileDb.DeleteRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
                }
            }

            return oldValue;
        }

        public void Flush()
        {
            try
            {
                fileDb.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("FileDbCache: FileDb.Flush() failed: {0}", ex.Message);
            }
        }

        public void Clean()
        {
            try
            {
                fileDb.Clean();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("FileDbCache: FileDb.Clean() failed: {0}", ex.Message);
            }
        }

        public void Dispose()
        {
            if (fileDb.IsOpen)
            {
                try
                {
                    fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, EqualityEnum.LessThanOrEqual));
                    Trace.TraceInformation("FileDbCache: Deleted {0} expired items", fileDb.NumDeleted);
                    fileDb.Clean();
                }
                catch
                {
                }

                fileDb.Close();
            }
        }

        private bool RepairDatabase()
        {
            try
            {
                fileDb.Reindex();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("FileDbCache: FileDb.Reindex() failed: {0}", ex.Message);
                return CreateDatabase();
            }

            return true;
        }

        private bool CreateDatabase()
        {
            if (fileDb.IsOpen)
            {
                fileDb.Close();
            }

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                fileDb.Create(path,
                    new Field[]
                {
                    new Field(keyField, DataTypeEnum.String) { IsPrimaryKey = true },
                    new Field(valueField, DataTypeEnum.Byte) { IsArray = true },
                    new Field(expiresField, DataTypeEnum.DateTime)
                });

                Trace.TraceInformation("FileDbCache: Created database {0}", path);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("FileDbCache: Creating database failed: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private void AddOrUpdateRecord(string key, object value, DateTime expires)
        {
            var fieldValues = new FieldValues(3); // capacity
            fieldValues.Add(valueField, value);
            fieldValues.Add(expiresField, expires);

            if (fileDb.GetRecordByKey(key, new string[0], false) == null)
            {
                fieldValues.Add(keyField, key);
                fileDb.AddRecord(fieldValues);
            }
            else
            {
                fileDb.UpdateRecordByKey(key, fieldValues);
            }
        }
    }
}
