// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
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
        private static double zoomLevelSwitchOffset = Math.Log(maxScaledTileSize / 256d, 2d);

        private Size size;
        private Point origin;
        private Point offset;
        private double rotation;
        private double zoomLevel;
        private int tileZoomLevel;
        private Int32Rect tileGrid;
        private readonly DispatcherTimer updateTimer;

        public readonly MatrixTransform ViewportTransform = new MatrixTransform();

        public TileContainer()
        {
            updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
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

        public double SetViewportTransform(double mapZoomLevel, double mapRotation, Point mapOrigin, Point viewportOrigin, Size viewportSize)
        {
            var scale = Math.Pow(2d, zoomLevel) * 256d / 360d;
            var oldMapOriginX = (origin.X - offset.X) / scale - 180d;

            if (zoomLevel != mapZoomLevel)
            {
                zoomLevel = mapZoomLevel;
                scale = Math.Pow(2d, zoomLevel) * 256d / 360d;
            }

            rotation = mapRotation;
            size = viewportSize;
            origin = viewportOrigin;

            offset.X = origin.X - (180d + mapOrigin.X) * scale;
            offset.Y = origin.Y - (180d - mapOrigin.Y) * scale;

            ViewportTransform.Matrix = GetTransformMatrix(new Matrix(1d, 0d, 0d, -1d, 180d, 180d), scale);

            if (Math.Sign(mapOrigin.X) == Math.Sign(oldMapOriginX))
            {
                var tileLayerTransform = GetTileLayerTransformMatrix();

                foreach (TileLayer tileLayer in Children)
                {
                    tileLayer.SetTransformMatrix(tileLayerTransform);
                }

                updateTimer.Start();
            }
            else
            {
                // immediately handle map origin leap when map center moves across the date line
                UpdateTiles(this, EventArgs.Empty);
            }

            return scale;
        }

        private Matrix GetTileLayerTransformMatrix()
        {
            // Calculates the TileLayer VisualTransform or RenderTransform matrix
            // with origin at tileGrid.X and tileGrid.Y to minimize rounding errors.

            return GetTransformMatrix(
                new Matrix(1d, 0d, 0d, 1d, tileGrid.X * 256d, tileGrid.Y * 256d),
                Math.Pow(2d, zoomLevel - tileZoomLevel));
        }

        private void UpdateTiles(object sender, object e)
        {
            updateTimer.Stop();

            var zoom = (int)Math.Floor(zoomLevel + 1d - zoomLevelSwitchOffset);
            var numTiles = 1 << zoom;
            var transform = GetTileIndexMatrix(numTiles);

            // tile indices of visible rectangle
            var p1 = transform.Transform(new Point(0d, 0d));
            var p2 = transform.Transform(new Point(size.Width, 0d));
            var p3 = transform.Transform(new Point(0d, size.Height));
            var p4 = transform.Transform(new Point(size.Width, size.Height));

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
