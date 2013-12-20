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
using System.Windows.Media;
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
        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            "RelativeImageSize", typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        private readonly DispatcherTimer updateTimer;
        private string uriFormat;
        private int currentImageIndex;
        private bool updateInProgress;

        public MapImageLayer()
        {
            Children.Add(new MapImage { Opacity = 0d });
            Children.Add(new MapImage { Opacity = 0d });

            updateTimer = new DispatcherTimer { Interval = TileContainer.UpdateInterval };
            updateTimer.Tick += UpdateImage;
        }

        /// <summary>
        /// Relative size of the map images in relation to the current viewport size.
        /// Setting a value greater than one will let MapImageLayer request images that
        /// are larger than the viewport, in order to support smooth panning. 
        /// </summary>
        public double RelativeImageSize
        {
            get { return (double)GetValue(RelativeImageSizeProperty); }
            set { SetValue(RelativeImageSizeProperty, value); }
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

                    if (!(value.Contains("{W}") && value.Contains("{S}") && value.Contains("{E}") && value.Contains("{N}")) &&
                        !(value.Contains("{w}") && value.Contains("{s}") && value.Contains("{e}") && value.Contains("{n}")))
                    {
                        throw new ArgumentException("UriFormat must specify a bounding box in meters by {W},{S},{E},{N} or lat/lon by {w},{s},{e},{n}.");
                    }
                }

                uriFormat = value;

                UpdateImage(this, EventArgs.Empty);
            }
        }

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            updateTimer.Stop();
            updateTimer.Start();
        }

        protected virtual ImageSource GetImage(double west, double east, double south, double north, int width, int height)
        {
            ImageSource image = null;

            if (uriFormat != null)
            {
                var uri = uriFormat.Replace("{X}", width.ToString()).Replace("{Y}", height.ToString());

                if (uri.Contains("{W}") && uri.Contains("{S}") && uri.Contains("{E}") && uri.Contains("{N}"))
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
                else
                {
                    uri = uri.
                        Replace("{w}", west.ToString(CultureInfo.InvariantCulture)).
                        Replace("{s}", south.ToString(CultureInfo.InvariantCulture)).
                        Replace("{e}", east.ToString(CultureInfo.InvariantCulture)).
                        Replace("{n}", north.ToString(CultureInfo.InvariantCulture));
                }

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = "XAML Map Control";

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        image = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("{0}: {1}", uri, ex.Message);
                }
            }

            return image;
        }

        private void UpdateImage(object sender, EventArgs e)
        {
            if (updateInProgress)
            {
                return; // update image on next timer tick
            }

            updateTimer.Stop();

            if (ParentMap != null && ActualWidth > 0 && ActualHeight > 0)
            {
                updateInProgress = true;

                var relativeSize = Math.Max(RelativeImageSize, 1d);
                var width = ActualWidth * relativeSize;
                var height = ActualHeight * relativeSize;
                var dx = (ActualWidth - width) / 2d;
                var dy = (ActualHeight - height) / 2d;

                var loc1 = ParentMap.ViewportPointToLocation(new Point(dx, dy));
                var loc2 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy));
                var loc3 = ParentMap.ViewportPointToLocation(new Point(dx, dy + height));
                var loc4 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy + height));

                ThreadPool.QueueUserWorkItem(o =>
                {
                    var west = Math.Min(loc1.Longitude, Math.Min(loc2.Longitude, Math.Min(loc3.Longitude, loc4.Longitude)));
                    var east = Math.Max(loc1.Longitude, Math.Max(loc2.Longitude, Math.Max(loc3.Longitude, loc4.Longitude)));
                    var south = Math.Min(loc1.Latitude, Math.Min(loc2.Latitude, Math.Min(loc3.Latitude, loc4.Latitude)));
                    var north = Math.Max(loc1.Latitude, Math.Max(loc2.Latitude, Math.Max(loc3.Latitude, loc4.Latitude)));

                    var p1 = ParentMap.MapTransform.Transform(new Location(south, west));
                    var p2 = ParentMap.MapTransform.Transform(new Location(north, east));

                    width = Math.Round((p2.X - p1.X) * ParentMap.ViewportScale);
                    height = Math.Round((p2.Y - p1.Y) * ParentMap.ViewportScale);

                    var image = GetImage(west, east, south, north, (int)width, (int)height);

                    Dispatcher.BeginInvoke(new Action(() => UpdateImage(west, east, south, north, image)));

                    updateInProgress = false;
                });
            }
        }

        private void UpdateImage(double west, double east, double south, double north, ImageSource image)
        {
            var mapImage = (MapImage)Children[currentImageIndex];
            mapImage.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation
                {
                    To = 0d,
                    Duration = Tile.AnimationDuration,
                    BeginTime = Tile.AnimationDuration
                });

            currentImageIndex = (currentImageIndex + 1) % 2;
            mapImage = (MapImage)Children[currentImageIndex];
            mapImage.Source = null;
            mapImage.North = double.NaN; // avoid frequent MapRectangle.UpdateData() calls
            mapImage.West = west;
            mapImage.East = east;
            mapImage.South = south;
            mapImage.North = north;
            mapImage.Source = image;
            mapImage.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1d, Tile.AnimationDuration));
        }
    }
}
