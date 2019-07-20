// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using FileDbNs;
using System;
using System.Diagnostics;
using System.IO;

namespace MapControl.Caching
{
    /// <summary>
    /// Image cache implementation based on FileDb, a free and simple No-SQL database by EzTools Software.
    /// See http://www.eztools-software.com/tools/filedb/.
    /// </summary>
    public sealed partial class FileDbCache : IDisposable
    {
        private const string keyField = "Key";
        private const string valueField = "Value";
        private const string expiresField = "Expires";

        private readonly FileDb fileDb = new FileDb() { AutoFlush = true };
        private readonly string dbPath;

        public void Dispose()
        {
            fileDb.Dispose();
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

        private void Open()
        {
            if (!fileDb.IsOpen)
            {
                try
                {
                    fileDb.Open(dbPath);
                    Debug.WriteLine("FileDbCache: Opened database " + dbPath);

                    Clean();
                }
                catch
                {
                    CreateDatabase();
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

        private Record GetRecordByKey(string key)
        {
            Record record = null;

            if (fileDb.IsOpen)
            {
                try
                {
                    record = fileDb.GetRecordByKey(key, new string[] { valueField, expiresField }, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache.GetRecordByKey(\"{0}\"): {1}", key, ex.Message);
                }
            }

            return record;
        }

        private void AddOrUpdateRecord(string key, byte[] value, DateTime expiration)
        {
            if (fileDb.IsOpen)
            {
                var fieldValues = new FieldValues(3);
                fieldValues.Add(valueField, value);
                fieldValues.Add(expiresField, expiration);

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

                    //Debug.WriteLine("FileDbCache: Writing \"{0}\", Expires {1}", key, imageCacheItem.Expiration.ToLocalTime());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("FileDbCache.AddOrUpdateRecord(\"{0}\"): {1}", key, ex.Message); return;
                }
            }
        }
    }
}
