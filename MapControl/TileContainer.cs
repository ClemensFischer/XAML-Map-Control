// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
#endif

namespace MapControl
{
    internal partial class TileContainer : PanelBase
    {
        // relative size of scaled tile ranges from 0.75 to 1.5 (192 to 384 pixels)
        private static double zoomLevelSwitchDelta = -Math.Log(0.75, 2d);

        private readonly DispatcherTimer updateTimer;
        private Size viewportSize;
        private Point viewportOrigin;
        private Point tileLayerOffset;
        private double rotation;
        private double zoomLevel;
        private int tileZoomLevel;
        private Int32Rect tileGrid;

        public readonly MatrixTransform ViewportTransform = new MatrixTransform();

        public TileContainer()
        {
            RenderTransform = new MatrixTransform();
            updateTimer = new DispatcherTimer { Interval = Settings.TileUpdateInterval };
            updateTimer.Tick += UpdateTiles;
        }

        public IEnumerable<TileLayer> TileLayers
        {
            get { return Children.Cast<TileLayer>(); }
        }

        public void AddTileLayers(int index, IEnumerable<TileLayer> tileLayers)
        {
            foreach (var tileLayer in tileLayers)
            {
                if (index < Children.Count)
                {
                    Children.Insert(index, tileLayer);
                }
                else
                {
                    Children.Add(tileLayer);
                }

                index++;
                tileLayer.UpdateTiles(tileZoomLevel, tileGrid);
            }
        }

        public void RemoveTileLayers(int index, int count)
        {
            while (count-- > 0)
            {
                ((TileLayer)Children[index]).ClearTiles();
                Children.RemoveAt(index);
            }
        }

        public void ClearTileLayers()
        {
            foreach (TileLayer tileLayer in Children)
            {
                tileLayer.ClearTiles();
            }

            Children.Clear();
        }

        public double SetViewportTransform(double mapZoomLevel, double mapRotation, Point mapOrigin, Point vpOrigin, Size vpSize)
        {
            var scale = Math.Pow(2d, zoomLevel) * TileSource.TileSize / 360d;
            var oldMapOriginX = (viewportOrigin.X - tileLayerOffset.X) / scale - 180d;

            if (zoomLevel != mapZoomLevel)
            {
                zoomLevel = mapZoomLevel;
                scale = Math.Pow(2d, zoomLevel) * TileSource.TileSize / 360d;
            }

            rotation = mapRotation;
            viewportSize = vpSize;
            viewportOrigin = vpOrigin;

            var transformOffsetX = viewportOrigin.X - mapOrigin.X * scale;
            var transformOffsetY = viewportOrigin.Y + mapOrigin.Y * scale;

            UpdateViewportTransform(scale, transformOffsetX, transformOffsetY);

            tileLayerOffset.X = transformOffsetX - 180d * scale;
            tileLayerOffset.Y = transformOffsetY - 180d * scale;

            UpdateRenderTransform();

            if (Math.Abs(mapOrigin.X - oldMapOriginX) > 180d)
            {
                // immediately handle map origin leap when map center moves across 180° longitude
                UpdateTiles(this, EventArgs.Empty);
            }
            else
            {
                updateTimer.Start();
            }

            return scale;
        }

        private void UpdateTiles(object sender, object e)
        {
            updateTimer.Stop();

            var zoom = (int)Math.Floor(zoomLevel + zoomLevelSwitchDelta);
            var scale = (double)(1 << zoom) / 360d;
            var transform = GetTileIndexMatrix(scale);

            // tile indices of visible rectangle
            var p1 = transform.Transform(new Point(0d, 0d));
            var p2 = transform.Transform(new Point(viewportSize.Width, 0d));
            var p3 = transform.Transform(new Point(0d, viewportSize.Height));
            var p4 = transform.Transform(new Point(viewportSize.Width, viewportSize.Height));

            // index ranges of visible tiles
            var x1 = (int)Math.Floor(Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X))));
            var y1 = (int)Math.Floor(Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y))));
            var x2 = (int)Math.Floor(Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X))));
            var y2 = (int)Math.Floor(Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y))));
            var grid = new Int32Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1);

            if (tileZoomLevel != zoom || tileGrid != grid)
            {
                tileZoomLevel = zoom;
                tileGrid = grid;

                UpdateRenderTransform();

                foreach (TileLayer tileLayer in Children)
                {
                    tileLayer.UpdateTiles(tileZoomLevel, tileGrid);
                }
            }
        }
    }
}
