﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl.MBTiles
{
    /// <summary>
    /// MapTileLayer that uses an MBTiles SQLite Database. See https://wiki.openstreetmap.org/wiki/MBTiles.
    /// </summary>
    public class MBTileLayer : MapTileLayer
    {
        public static readonly DependencyProperty FileProperty =
            DependencyPropertyHelper.Register<MBTileLayer, string>(nameof(File), null,
                async (layer, oldValue, newValue) => await layer.FilePropertyChanged(newValue));

        public string File
        {
            get => (string)GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        /// <summary>
        /// May be overridden to create a derived MBTileSource that handles other tile formats than png and jpg.
        /// </summary>
        protected virtual MBTileSource CreateTileSource(MBTileData tileData)
        {
            return new MBTileSource(tileData);
        }

        private async Task FilePropertyChanged(string file)
        {
            (TileSource as MBTileSource)?.Dispose();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);

            if (file != null)
            {
                var tileData = await MBTileData.CreateAsync(file);

                if (tileData.Metadata.TryGetValue("name", out string sourceName))
                {
                    SourceName = sourceName;
                }

                if (tileData.Metadata.TryGetValue("description", out string description))
                {
                    Description = description;
                }

                if (tileData.Metadata.TryGetValue("minzoom", out sourceName) && int.TryParse(sourceName, out int minZoom))
                {
                    MinZoomLevel = minZoom;
                }

                if (tileData.Metadata.TryGetValue("maxzoom", out sourceName) && int.TryParse(sourceName, out int maxZoom))
                {
                    MaxZoomLevel = maxZoom;
                }

                TileSource = CreateTileSource(tileData);
            }
        }
    }
}
