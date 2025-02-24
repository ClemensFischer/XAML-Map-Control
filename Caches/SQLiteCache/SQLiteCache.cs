// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// IDistributedCache implementation based on System.Data.SQLite.
    /// </summary>
    public sealed class SQLiteCache : IDistributedCache, IDisposable
    {
        private readonly SQLiteConnection connection;
        private readonly Timer expirationScanTimer;

        public SQLiteCache(string path)
            : this(path, TimeSpan.FromHours(1))
        {
        }

        public SQLiteCache(string path, TimeSpan expirationScanFrequency)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The {nameof(path)} argument must not be null or empty.", nameof(path));
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path, "TileCache.sqlite");
            }

            connection = new SQLiteConnection("Data Source=" + Path.GetFullPath(path));
            connection.Open();

            using (var command = new SQLiteCommand("pragma journal_mode=wal", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand("create table if not exists items (key text primary key, expiration integer, buffer blob)", connection))
            {
                command.ExecuteNonQuery();
            }

            Debug.WriteLine($"{nameof(SQLiteCache)}: Opened database {path}");

            if (expirationScanFrequency > TimeSpan.Zero)
            {
                expirationScanTimer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, expirationScanFrequency);
            }
        }

        public void Dispose()
        {
            expirationScanTimer?.Dispose();
            connection.Dispose();
        }

        public byte[] Get(string key)
        {
            CheckArgument(key);

            byte[] value = null;

            try
            {
                using (var command = GetItemCommand(key))
                {
                    value = (byte[])command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.Get({key}): {ex.Message}");
            }

            return value;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            CheckArgument(key);

            byte[] value = null;

            try
            {
                using (var command = GetItemCommand(key))
                {
                    value = (byte[])await command.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.GetAsync({key}): {ex.Message}");
            }

            return value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CheckArguments(key, value, options);

            try
            {
                using (var command = SetItemCommand(key, value, options))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.Set({key}): {ex.Message}");
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            CheckArguments(key, value, options);

            try
            {
                using (var command = SetItemCommand(key, value, options))
                {
                    await command.ExecuteNonQueryAsync(token);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.SetAsync({key}): {ex.Message}");
            }
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
            CheckArgument(key);

            try
            {
                using (var command = DeleteItemCommand(key))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.Remove({key}): {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            CheckArgument(key);

            try
            {
                using (var command = DeleteItemCommand(key))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(SQLiteCache)}.RemoveAsync({key}): {ex.Message}");
            }
        }

        public void DeleteExpiredItems()
        {
            using (var command = DeleteExpiredItemCommand())
            {
                var deleted = (long)command.ExecuteScalar();
                if (deleted > 0)
                {
                    Debug.WriteLine($"{nameof(SQLiteCache)}: Deleted {deleted} expired items");
                }
            }
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

        private SQLiteCommand GetItemCommand(string key)
        {
            var command = new SQLiteCommand("select buffer from items where key = @key and expiration > @now", connection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.Ticks);
            return command;
        }

        private SQLiteCommand SetItemCommand(string key, byte[] buffer, DistributedCacheEntryOptions options)
        {
            var expiration = options.AbsoluteExpiration ??
                DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration ?? TimeSpan.FromDays(1));

            var command = new SQLiteCommand("insert or replace into items (key, expiration, buffer) values (@key, @exp, @buf)", connection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@exp", expiration.UtcTicks);
            command.Parameters.AddWithValue("@buf", buffer);
            return command;
        }

        private SQLiteCommand DeleteItemCommand(string key)
        {
            var command = new SQLiteCommand("delete from items where key = @key", connection);
            command.Parameters.AddWithValue("@key", key);
            return command;
        }

        private SQLiteCommand DeleteExpiredItemCommand()
        {
            var command = new SQLiteCommand("delete from items where expiration <= @now; select changes()", connection);
            command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.Ticks);
            return command;
        }
    }
}
