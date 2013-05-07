// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    /// <summary>
    /// Map image overlay. Fills the entire viewport with a map image from a web request,
    /// for example from a Web Map Service (WMS).
    /// The request Uri is specified by the UriFormat property, which has {X} and {Y}
    /// format specifiers for the map width and height in pixels, and either
    /// {w},{s},{e},{n} for the bounding box in lat/lon (like for example EPSG:4326) or
    /// {W},{S},{E},{N} for the bounding box in meters (like for example EPSG:3857)
    /// </summary>
    public class MapImageLayer : MapPanel
    {
        private readonly DispatcherTimer updateTimer;
        private readonly WebClient webClient;
        private string uriFormat;
        private bool latLonBoundingBox;
        private bool imageIsValid;
        private bool updateInProgress;
        private int currentImageIndex;

        public MapImageLayer()
        {
            Children.Add(new MapImage { Opacity = 0d });
            Children.Add(new MapImage { Opacity = 0d });

            webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.UserAgent, "XAML Map Control");

            updateTimer = new DispatcherTimer { Interval = TileContainer.UpdateInterval };
            updateTimer.Tick += UpdateImage;
        }

        public string UriFormat
        {
            get { return uriFormat; }
            set
            {
                if (value != null)
                {
                    if (!(value.Contains("{X}") && value.Contains("{Y}")))
                    {
                        throw new ArgumentException("UriFormat must specify the requested image size by {X} and {Y}.");
                    }

                    if (value.Contains("{w}") && value.Contains("{s}") && value.Contains("{e}") && value.Contains("{n}"))
                    {
                        latLonBoundingBox = true;
                    }
                    else if (!(value.Contains("{W}") && value.Contains("{S}") && value.Contains("{E}") && value.Contains("{N}")))
                    {
                        throw new ArgumentException("UriFormat must specify a bounding box in meters by {W},{S},{E},{N} or as lat/lon by {w},{s},{e},{n}.");
                    }
                }

                uriFormat = value;

                if (ParentMap != null)
                {
                    UpdateImage(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            imageIsValid = false;
            updateTimer.Stop();
            updateTimer.Start();
        }

        private void UpdateImage(object sender, EventArgs e)
        {
            updateTimer.Stop();

            if (updateInProgress || string.IsNullOrWhiteSpace(uriFormat))
            {
                return;
            }

            imageIsValid = true;
            updateInProgress = true;

            var uri = uriFormat.
                Replace("{X}", ActualWidth.ToString("F0", CultureInfo.InvariantCulture)).
                Replace("{Y}", ActualHeight.ToString("F0", CultureInfo.InvariantCulture));

            var loc1 = ParentMap.ViewportPointToLocation(new Point(0d, 0d));
            var loc2 = ParentMap.ViewportPointToLocation(new Point(ActualWidth, 0d));
            var loc3 = ParentMap.ViewportPointToLocation(new Point(0d, ActualHeight));
            var loc4 = ParentMap.ViewportPointToLocation(new Point(ActualWidth, ActualHeight));

            ThreadPool.QueueUserWorkItem(o =>
            {
                var west = Math.Min(loc1.Longitude, Math.Min(loc2.Longitude, Math.Min(loc3.Longitude, loc4.Longitude)));
                var east = Math.Max(loc1.Longitude, Math.Max(loc2.Longitude, Math.Max(loc3.Longitude, loc4.Longitude)));
                var south = Math.Min(loc1.Latitude, Math.Min(loc2.Latitude, Math.Min(loc3.Latitude, loc4.Latitude)));
                var north = Math.Max(loc1.Latitude, Math.Max(loc2.Latitude, Math.Max(loc3.Latitude, loc4.Latitude)));

                if (latLonBoundingBox)
                {
                    uri = uri.
                        Replace("{w}", west.ToString(CultureInfo.InvariantCulture)).
                        Replace("{s}", south.ToString(CultureInfo.InvariantCulture)).
                        Replace("{e}", east.ToString(CultureInfo.InvariantCulture)).
                        Replace("{n}", north.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    var p1 = ParentMap.MapTransform.Transform(new Location(south, west));
                    var p2 = ParentMap.MapTransform.Transform(new Location(north, east));
                    var arc = TileSource.EarthRadius * Math.PI / 180d;

                    uri = uri.
                        Replace("{W}", (arc * p1.X).ToString(CultureInfo.InvariantCulture)).
                        Replace("{S}", (arc * p1.Y).ToString(CultureInfo.InvariantCulture)).
                        Replace("{E}", (arc * p2.X).ToString(CultureInfo.InvariantCulture)).
                        Replace("{N}", (arc * p2.Y).ToString(CultureInfo.InvariantCulture));
                }

                try
                {
                    var buffer = webClient.DownloadData(uri);
                    var bitmap = new BitmapImage();

                    using (var stream = new MemoryStream(buffer))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var image = (MapImage)Children[currentImageIndex];
                        image.BeginAnimation(Image.OpacityProperty,
                            new DoubleAnimation(0d, Tile.AnimationDuration)
                            {
                                BeginTime = Tile.AnimationDuration
                            });

                        currentImageIndex = (currentImageIndex + 1) % 2;
                        image = (MapImage)Children[currentImageIndex];
                        image.Source = null;
                        image.North = double.NaN; // avoid frequent MapRectangle.UpdateGeometry() calls
                        image.West = west;
                        image.East = east;
                        image.South = south;
                        image.North = north;
                        image.Source = bitmap;
                        image.BeginAnimation(Image.OpacityProperty, new DoubleAnimation(1d, Tile.AnimationDuration));

                        if (!imageIsValid)
                        {
                            UpdateImage(this, EventArgs.Empty);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("{0}: {1}", uri, ex.Message);
                }

                updateInProgress = false;
            });
        }
    }
}
