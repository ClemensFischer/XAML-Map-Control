﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder where the Cache instance may save data.
        /// </summary>
        public static StorageFolder DefaultCacheFolder => ApplicationData.Current.LocalCacheFolder;

        private static async Task LoadTileAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            async void callback()
            {
                try
                {
                    tile.SetImageSource(await loadImageFunc());
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, callback);

            await tcs.Task.ConfigureAwait(false);
        }
    }
}
