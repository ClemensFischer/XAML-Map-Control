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
        public MBTileData TileData { get; }
        public string Name { get; }
        public string Description { get; }
        public string Format { get; }
        public int? MinZoom { get; }
        public int? MaxZoom { get; }

        private MBTileSource(MBTileData tiledata, IDictionary<string, string> metadata)
        {
            TileData = tiledata;

            string s;
            int minZoom;
            int maxZoom;

            if (metadata.TryGetValue("name", out s))
            {
                Name = s;
            }

            if (metadata.TryGetValue("description", out s))
            {
                Description = s;
            }

            if (metadata.TryGetValue("format", out s))
            {
                Format = s;
            }

            if (metadata.TryGetValue("minzoom", out s) && int.TryParse(s, out minZoom))
            {
                MinZoom = minZoom;
            }

            if (metadata.TryGetValue("maxzoom", out s) && int.TryParse(s, out maxZoom))
            {
                MaxZoom = maxZoom;
            }
        }

        public static async Task<MBTileSource> CreateAsync(string file)
        {
            var tiledata = await MBTileData.CreateAsync(file);
            var metadata = await tiledata.ReadMetadataAsync();

            return new MBTileSource(tiledata, metadata);
        }

        public void Dispose()
        {
            TileData.Dispose();
        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            var buffer = await TileData.ReadImageBufferAsync(x, y, zoomLevel);

            return buffer != null ? await ImageLoader.LoadImageAsync(buffer) : null;
        }
    }
}
