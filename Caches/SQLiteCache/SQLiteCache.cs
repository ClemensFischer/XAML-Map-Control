using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public class SQLiteCacheOptions : IOptions<SQLiteCacheOptions>
    {
        public SQLiteCacheOptions Value => this;

        public string Path { get; set; }

        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// IDistributedCache implementation based on System.Data.SQLite, https://system.data.sqlite.org/.
    /// </summary>
    public sealed class SQLiteCache : IDistributedCache, IDisposable
    {
        private readonly SQLiteConnection connection;
        private readonly Timer timer;
        private readonly ILogger logger;

        public SQLiteCache(string path, ILoggerFactory loggerFactory = null)
            : this(new SQLiteCacheOptions { Path = path }, loggerFactory)
        {
        }

        public SQLiteCache(IOptions<SQLiteCacheOptions> optionsAccessor, ILoggerFactory loggerFactory = null)
            : this(optionsAccessor.Value, loggerFactory)
        {
        }

        public SQLiteCache(SQLiteCacheOptions options, ILoggerFactory loggerFactory = null)
        {
            var path = options.Path;

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path ?? "", "TileCache.sqlite");
            }

            connection = new SQLiteConnection("Data Source=" + path);
            connection.Open();

            using (var command = new SQLiteCommand("pragma journal_mode=wal", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand("create table if not exists items (key text primary key, expiration integer, buffer blob)", connection))
            {
                command.ExecuteNonQuery();
            }

            logger = loggerFactory?.CreateLogger<SQLiteCache>();

            logger?.LogInformation("Opened database {path}", path);

            if (options.ExpirationScanFrequency > TimeSpan.Zero)
            {
                timer = new Timer(_ => DeleteExpiredItems(), null, TimeSpan.Zero, options.ExpirationScanFrequency);
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
            connection.Dispose();
        }

        public byte[] Get(string key)
        {
            byte[] value = null;

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    using (var command = GetItemCommand(key))
                    {
                        value = (byte[])command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Get({key})", key);
                }
            }

            return value;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            byte[] value = null;

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    using (var command = GetItemCommand(key))
                    {
                        value = (byte[])await command.ExecuteScalarAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "GetAsync({key})", key);
                }
            }

            return value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (!string.IsNullOrEmpty(key) && value != null && options != null)
            {
                try
                {
                    using (var command = SetItemCommand(key, value, options))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Set({key})", key);
                }
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(key) && value != null && options != null)
            {
                try
                {
                    using (var command = SetItemCommand(key, value, options))
                    {
                        await command.ExecuteNonQueryAsync(token);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "SetAsync({key})", key);
                }
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
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    using (var command = DeleteItemCommand(key))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Remove({key})", key);
                }
            }
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    using (var command = DeleteItemCommand(key))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "RemoveAsync({key})", key);
                }
            }
        }

        public void DeleteExpiredItems()
        {
            long deletedItemsCount;

            using (var command = DeleteExpiredItemsCommand())
            {
                deletedItemsCount = (long)command.ExecuteScalar();
            }

            if (deletedItemsCount > 0)
            {
                using (var command = new SQLiteCommand("vacuum", connection))
                {
                    command.ExecuteNonQuery();
                }

                logger?.LogInformation("Deleted {count} expired items", deletedItemsCount);
            }
        }

        private SQLiteCommand GetItemCommand(string key)
        {
            var command = new SQLiteCommand("select buffer from items where key = @key and expiration > @exp", connection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@exp", DateTimeOffset.UtcNow.Ticks);
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

        private SQLiteCommand DeleteExpiredItemsCommand()
        {
            var command = new SQLiteCommand("delete from items where expiration <= @exp; select changes()", connection);
            command.Parameters.AddWithValue("@exp", DateTimeOffset.UtcNow.Ticks);
            return command;
        }
    }
}
