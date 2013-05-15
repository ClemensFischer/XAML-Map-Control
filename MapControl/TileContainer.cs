// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
#if NETFX_CORE
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
    internal partial class TileContainer
    {
        private const double maxScaledTileSize = 400d; // scaled tile size 200..400 units
        private static double zoomLevelSwitchDelta = Math.Log(maxScaledTileSize / TileSource.TileSize, 2d);

        internal static TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5);

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
            updateTimer = new DispatcherTimer { Interval = UpdateInterval };
            updateTimer.Tick += UpdateTiles;
        }

        public void AddTileLayers(int index, IEnumerable<TileLayer> tileLayers)
        {
            var tileLayerTransform = GetTileLayerTransformMatrix();

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
                tileLayer.SetTransformMatrix(tileLayerTransform);
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

            tileLayerOffset.X = transformOffsetX - 180d * scale;
            tileLayerOffset.Y = transformOffsetY - 180d * scale;

            SetViewportTransform(new Matrix(scale, 0d, 0d, -scale, transformOffsetX, transformOffsetY));

            if (Math.Sign(mapOrigin.X) != Math.Sign(oldMapOriginX) && Math.Abs(mapOrigin.X) > 90d)
            {
                // immediately handle map origin leap when map center moves across the date line
                UpdateTiles(this, EventArgs.Empty);
            }
            else
            {
                var tileLayerTransform = GetTileLayerTransformMatrix();

                foreach (TileLayer tileLayer in Children)
                {
                    tileLayer.SetTransformMatrix(tileLayerTransform);
                }

                updateTimer.Start();
            }

            return scale;
        }

        private void UpdateTiles(object sender, object e)
        {
            updateTimer.Stop();

            var zoom = (int)Math.Floor(zoomLevel + 1d - zoomLevelSwitchDelta);
            var numTiles = 1 << zoom;
            var transform = GetTileIndexMatrix(numTiles);

            // tile indices of visible rectangle
            var p1 = transform.Transform(new Point(0d, 0d));
            var p2 = transform.Transform(new Point(viewportSize.Width, 0d));
            var p3 = transform.Transform(new Point(0d, viewportSize.Height));
            var p4 = transform.Transform(new Point(viewportSize.Width, viewportSize.Height));

            var left = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            var right = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X)));
            var top = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            var bottom = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y)));

            // index ranges of visible tiles
            var x1 = (int)Math.Floor(left);
            var x2 = (int)Math.Floor(right);
            var y1 = Math.Max((int)Math.Floor(top), 0);
            var y2 = Math.Min((int)Math.Floor(bottom), numTiles - 1);
            var grid = new Int32Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1);

            if (tileZoomLevel != zoom || tileGrid != grid)
            {
                tileZoomLevel = zoom;
                tileGrid = grid;
                var tileLayerTransform = GetTileLayerTransformMatrix();

                foreach (TileLayer tileLayer in Children)
                {
                    tileLayer.SetTransformMatrix(tileLayerTransform);
                    tileLayer.UpdateTiles(tileZoomLevel, tileGrid);
                }
            }
        }
    }
}
