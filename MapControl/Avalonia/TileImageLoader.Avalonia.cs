// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder where the Cache instance may save data, i.e. "C:\ProgramData\MapControl\TileCache".
        /// </summary>
        public static string DefaultCacheFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache");


        private static async Task LoadTileAsync(Tile tile, Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() => tile.SetImageSource(image));
        }
    }
}
