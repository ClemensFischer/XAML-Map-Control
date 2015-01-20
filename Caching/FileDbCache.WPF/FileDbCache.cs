// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Windows.Media.Imaging;
using FileDbNs;

namespace MapControl.Caching
{
    /// <summary>
    /// ObjectCache implementation based on FileDb, a free and simple No-SQL database by EzTools Software.
    /// See http://www.eztools-software.com/tools/filedb/.
    /// The only valid data type for cached values is System.Windows.Media.Imaging.BitmapFrame.
    /// </summary>
    public class FileDbCache : ObjectCache, IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb { AutoFlush = true, AutoCleanThreshold = -1 };
        private readonly string name;
        private readonly string path;

        public FileDbCache(string name, NameValueCollection config)
            : this(name, config["folder"])
        {
            var autoFlush = config["autoFlush"];
            var autoCleanThreshold = config["autoCleanThreshold"];

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

        public FileDbCache(string name, string folder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter name must not be null or empty or consist only of white-space characters.");
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("The parameter folder must not be null or empty or consist only of white-space characters.");
            }

            this.name = name;
            path = Path.Combine(folder, name);

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path += ".fdb";
            }

            try
            {
                fileDb.Open(path, false);
                Debug.WriteLine("FileDbCache: Opened database with {0} cached items in {1}.", fileDb.NumRecords, path);

                Clean();
            }
            catch
            {
                CreateDatabase();
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
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

            if (fileDb.IsOpen)
            {
                try
                {
                    return fileDb.NumRecords;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.NumRecords failed: {0}", (object)ex.Message);
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
                    Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
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
                Record record = null;

                try
                {
                    record = fileDb.GetRecordByKey(key, new string[] { valueField }, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
                }

                if (record != null)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream((byte[])record[0]))
                        {
                            return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("FileDbCache: Decoding \"{0}\" failed: {1}", key, ex.Message);
                    }

                    try
                    {
                        fileDb.DeleteRecordByKey(key);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("FileDbCache: FileDb.DeleteRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
                    }
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

            if (policy == null)
            {
                throw new ArgumentNullException("The parameter policy must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var bitmap = value as BitmapFrame;

            if (bitmap == null)
            {
                throw new ArgumentException("The parameter value must contain a System.Windows.Media.Imaging.BitmapFrame.");
            }

            if (fileDb.IsOpen)
            {
                byte[] buffer = null;

                try
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(bitmap);

                    using (var memoryStream = new MemoryStream())
                    {
                        encoder.Save(memoryStream);
                        buffer = memoryStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache: Encoding \"{0}\" failed: {1}", key, ex.Message);
                }

                if (buffer != null)
                {
                    var expires = DateTime.MaxValue;

                    if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration)
                    {
                        expires = policy.AbsoluteExpiration.DateTime;
                    }
                    else if (policy.SlidingExpiration != NoSlidingExpiration)
                    {
                        expires = DateTime.UtcNow + policy.SlidingExpiration;
                    }

                    if (!AddOrUpdateRecord(key, buffer, expires) && RepairDatabase())
                    {
                        AddOrUpdateRecord(key, buffer, expires);
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
                    Debug.WriteLine("FileDbCache: FileDb.DeleteRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
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
                fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThan));

                if (fileDb.NumDeleted > 0)
                {
                    Debug.WriteLine("FileDbCache: Deleted {0} expired items.", fileDb.NumDeleted);
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

            Debug.WriteLine("FileDbCache: Created database {0}.", (object)path);
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
                Debug.WriteLine("FileDbCache: FileDb.Reindex() failed: {0}", (object)ex.Message);
            }

            try
            {
                CreateDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDbCache: Creating database {0} failed: {1}", path, ex.Message);
            }

            return false;
        }

        private bool AddOrUpdateRecord(string key, byte[] value, DateTime expires)
        {
            var fieldValues = new FieldValues(3);
            fieldValues.Add(valueField, value);
            fieldValues.Add(expiresField, expires);

            bool recordExists;

            try
            {
                recordExists = fileDb.GetRecordByKey(key, new string[0], false) != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FileDbCache: FileDb.GetRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
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
                    Debug.WriteLine("FileDbCache: FileDb.UpdateRecordByKey(\"{0}\") failed: {1}", key, ex.Message);
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
                    Debug.WriteLine("FileDbCache: FileDb.AddRecord(\"{0}\") failed: {1}", key, ex.Message);
                    return false;
                }
            }

            return true;
        }
    }
}
