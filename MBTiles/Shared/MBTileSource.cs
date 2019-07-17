// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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

        public MBTileSource(string file)
        {
            tileData = new MBTileData(file);
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int? MinZoom { get; private set; }
        public int? MaxZoom { get; private set; }

        public async Task Initialize()
        {
            await tileData.OpenAsync();

            var metadata = await tileData.ReadMetadataAsync();

            string s;
            int minZoom;
            int maxZoom;

            Name = (metadata.TryGetValue("name", out s)) ? s : null;

            Description = (metadata.TryGetValue("description", out s)) ? s : null;

            if (metadata.TryGetValue("minzoom", out s) && int.TryParse(s, out minZoom))
            {
                MinZoom = minZoom;
            }
            else
            {
                MinZoom = null;
            }

            if (metadata.TryGetValue("maxzoom", out s) && int.TryParse(s, out maxZoom))
            {
                MaxZoom = maxZoom;
            }
            else
            {
                MaxZoom = null;
            }
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
