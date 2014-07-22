// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    /// <summary>
    /// Map image overlay. Fills the entire viewport with a map image from a web request,
    /// for example from a Web Map Service (WMS).
    /// The image request Uri is specified by the UriFormat property.
    /// </summary>
    public partial class MapImageLayer : MapPanel
    {
        public static readonly DependencyProperty UriFormatProperty = DependencyProperty.Register(
            "UriFormat", typeof(string), typeof(MapImageLayer),
            new PropertyMetadata(null, (o, e) => ((MapImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            "RelativeImageSize", typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        private int currentImageIndex;
        private bool updateInProgress;

        public MapImageLayer()
        {
            Children.Add(new MapImage { Opacity = 0d });
            Children.Add(new MapImage { Opacity = 0d });

            updateTimer.Interval = TileContainer.UpdateInterval;
            updateTimer.Tick += (s, e) => UpdateImage();
        }

        /// <summary>
        /// The format string of the image request Uri. The format must contain {X} and {Y}
        /// format specifiers for the map width and height in pixels, and either
        /// {w},{s},{e},{n} for the bounding box in lat/lon (like for example EPSG:4326) or
        /// {W},{S},{E},{N} for the bounding box in meters (like for example EPSG:3857).
        /// </summary>
        public string UriFormat
        {
            get { return (string)GetValue(UriFormatProperty); }
            set { SetValue(UriFormatProperty, value); }
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

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            updateTimer.Stop();
            updateTimer.Start();
        }

        protected virtual BitmapSource GetBitmap(double west, double east, double south, double north, int width, int height)
        {
            BitmapImage image = null;

            if (UriFormat != null && width > 0 && height > 0)
            {
                var uri = UriFormat.Replace("{X}", width.ToString()).Replace("{Y}", height.ToString());

                if (uri.Contains("{W}") && uri.Contains("{S}") && uri.Contains("{E}") && uri.Contains("{N}"))
                {
                    var p1 = ParentMap.MapTransform.Transform(new Location(south, west));
                    var p2 = ParentMap.MapTransform.Transform(new Location(north, east));

                    uri = uri.
                        Replace("{W}", (TileSource.MetersPerDegree * p1.X).ToString(CultureInfo.InvariantCulture)).
                        Replace("{S}", (TileSource.MetersPerDegree * p1.Y).ToString(CultureInfo.InvariantCulture)).
                        Replace("{E}", (TileSource.MetersPerDegree * p2.X).ToString(CultureInfo.InvariantCulture)).
                        Replace("{N}", (TileSource.MetersPerDegree * p2.Y).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    uri = uri.
                        Replace("{w}", west.ToString(CultureInfo.InvariantCulture)).
                        Replace("{s}", south.ToString(CultureInfo.InvariantCulture)).
                        Replace("{e}", east.ToString(CultureInfo.InvariantCulture)).
                        Replace("{n}", north.ToString(CultureInfo.InvariantCulture));
                }

                image = new BitmapImage(new Uri(uri));
            }

            return image;
        }

        protected void UpdateImage()
        {
            if (updateInProgress)
            {
                updateTimer.Start(); // update image on next timer tick
            }
            else
            {
                updateTimer.Stop();

                if (ParentMap != null && RenderSize.Width > 0 && RenderSize.Height > 0)
                {
                    updateInProgress = true;

                    var relativeSize = Math.Max(RelativeImageSize, 1d);
                    var width = RenderSize.Width * relativeSize;
                    var height = RenderSize.Height * relativeSize;
                    var dx = (RenderSize.Width - width) / 2d;
                    var dy = (RenderSize.Height - height) / 2d;

                    var loc1 = ParentMap.ViewportPointToLocation(new Point(dx, dy));
                    var loc2 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy));
                    var loc3 = ParentMap.ViewportPointToLocation(new Point(dx, dy + height));
                    var loc4 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy + height));

                    var west = Math.Min(loc1.Longitude, Math.Min(loc2.Longitude, Math.Min(loc3.Longitude, loc4.Longitude)));
                    var east = Math.Max(loc1.Longitude, Math.Max(loc2.Longitude, Math.Max(loc3.Longitude, loc4.Longitude)));
                    var south = Math.Min(loc1.Latitude, Math.Min(loc2.Latitude, Math.Min(loc3.Latitude, loc4.Latitude)));
                    var north = Math.Max(loc1.Latitude, Math.Max(loc2.Latitude, Math.Max(loc3.Latitude, loc4.Latitude)));

                    var p1 = ParentMap.MapTransform.Transform(new Location(south, west));
                    var p2 = ParentMap.MapTransform.Transform(new Location(north, east));

                    width = Math.Round((p2.X - p1.X) * ParentMap.ViewportScale);
                    height = Math.Round((p2.Y - p1.Y) * ParentMap.ViewportScale);

                    var image = GetBitmap(west, east, south, north, (int)width, (int)height);

                    UpdateImage(west, east, south, north, image);
                }
            }
        }

        private void UpdateImage(double west, double east, double south, double north, BitmapSource image)
        {
            currentImageIndex = (currentImageIndex + 1) % 2;
            var mapImage = (MapImage)Children[currentImageIndex];

            mapImage.Source = null;
            mapImage.North = double.NaN; // avoid frequent MapRectangle.UpdateData() calls
            mapImage.West = west;
            mapImage.East = east;
            mapImage.South = south;
            mapImage.North = north;

            if (image != null)
            {
                mapImage.Source = image;
                AddDownloadEventHandlers(image);
            }
            else
            {
                BlendImages();
            }
        }

        private void BlendImages()
        {
#if WINDOWS_RUNTIME
            var duration = TimeSpan.Zero; // animation not working in Windows Runtime (?)
#else
            var duration = Tile.AnimationDuration;
#endif
            var mapImage = (MapImage)Children[currentImageIndex];
            var fadeOut = new DoubleAnimation { To = 0d, Duration = duration };

            if (mapImage.Source != null)
            {
                mapImage.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation { To = 1d, Duration = duration });

                fadeOut.BeginTime = duration;
            }

            mapImage = (MapImage)Children[(currentImageIndex + 1) % 2];
            mapImage.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            updateInProgress = false;
        }
    }
}
