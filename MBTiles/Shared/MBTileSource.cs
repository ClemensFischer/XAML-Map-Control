// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace MapControl.MBTiles
{
    public sealed class MBTileSource : TileSource, IDisposable
    {
        private readonly MBTileData tileData;

        public string Name { get; }
        public string Description { get; }
        public int? MinZoom { get; }
        public int? MaxZoom { get; }

        private MBTileSource(MBTileData tileData, IDictionary<string, string> metaData)
        {
            this.tileData = tileData;

            string s;
            int minZoom;
            int maxZoom;

            if (metaData.TryGetValue("name", out s))
            {
                Name = s;
            }

            if (metaData.TryGetValue("description", out s))
            {
                Description = s;
            }

            if (metaData.TryGetValue("minzoom", out s) && int.TryParse(s, out minZoom))
            {
                MinZoom = minZoom;
            }

            if (metaData.TryGetValue("maxzoom", out s) && int.TryParse(s, out maxZoom))
            {
                MaxZoom = maxZoom;
            }
        }

        public static async Task<MBTileSource> CreateAsync(string file)
        {
            var tileData = await MBTileData.CreateAsync(file);

            return new MBTileSource(tileData, await tileData.ReadMetaDataAsync());
        }

        public void Dispose()
        {
            tileData.Dispose();
        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            var buffer = await tileData.ReadImageBufferAsync(x, y, zoomLevel);

            return buffer != null ? await ImageLoader.LoadImageAsync(buffer) : null;
        }
    }
}
