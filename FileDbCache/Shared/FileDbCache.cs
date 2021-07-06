// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
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

        private readonly FileDb fileDb = new FileDb { AutoFlush = true };

        public FileDbCache(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The path argument must not be null or empty.", nameof(path));
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path, "TileCache.fdb");
            }

            Open(path);
        }

        public void Dispose()
        {
            fileDb.Dispose();
        }

        public void Clean()
        {
            var deleted = fileDb.DeleteRecords(new FilterExpression(expiresField, DateTime.UtcNow, ComparisonOperatorEnum.LessThan));

            if (deleted > 0)
            {
                Debug.WriteLine($"FileDbCache: Deleted {deleted} expired items");
                fileDb.Clean();
            }
        }

        private void Open(string path)
        {
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

        private Record GetRecordByKey(string key)
        {
            try
            {
                return fileDb.GetRecordByKey(key, new string[] { valueField, expiresField }, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileDbCache.GetRecordByKey({key}): {ex.Message}");
            }

            return null;
        }

        private void AddOrUpdateRecord(string key, byte[] buffer, DateTime expiration)
        {
            var fieldValues = new FieldValues(3);
            fieldValues.Add(valueField, buffer ?? new byte[0]);
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileDbCache.AddOrUpdateRecord({key}): {ex.Message}");
            }
        }
    }
}
