// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace MapControl
{
    public class MBTileSource : TileSource, IDisposable
    {
        private readonly MBTileData tileData;

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int? MinZoom { get; private set; }
        public int? MaxZoom { get; private set; }

        public MBTileSource(string file)
        {
            tileData = new MBTileData(file);
        }

        public async Task Initialize()
        {
            await tileData.OpenAsync();

            var metadata = await tileData.ReadMetadataAsync();

            string name;
            string description;
            string minzoom;
            string maxzoom;
            int minZoom;
            int maxZoom;

            if (metadata.TryGetValue("name", out name))
            {
                Name = name;
            }

            if (metadata.TryGetValue("description", out description))
            {
                Description = description;
            }

            if (metadata.TryGetValue("minzoom", out minzoom) && int.TryParse(minzoom, out minZoom))
            {
                MinZoom = minZoom;
            }

            if (metadata.TryGetValue("maxzoom", out maxzoom) && int.TryParse(maxzoom, out maxZoom))
            {
                MaxZoom = maxZoom;
            }
        }

        public void Dispose()
        {
            tileData.Dispose();
        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            var buffer = await tileData.ReadImageBufferAsync(x, y, zoomLevel);

            return buffer != null ? await ImageLoader.CreateImageSourceAsync(buffer) : null;
        }
    }
}
