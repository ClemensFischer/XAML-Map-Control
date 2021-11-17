// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public partial class FileDbCache : IImageCache
    {
        public Task<Tuple<byte[], DateTime>> GetAsync(string key)
        {
            return Task.Run(() =>
            {
                var record = GetRecordByKey(key);

                if (record == null)
                {
                    return null;
                }

                return Tuple.Create((byte[])record[0], (DateTime)record[1]);
            });
        }

        public Task SetAsync(string key, byte[] buffer, DateTime expiration)
        {
            return Task.Run(() => AddOrUpdateRecord(key, buffer, expiration));
        }
    }
}
