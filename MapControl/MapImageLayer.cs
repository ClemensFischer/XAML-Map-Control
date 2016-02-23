// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Map image overlay. Fills the entire viewport with map images provided by a web service,
    /// e.g. a Web Map Service (WMS). The image request Uri is specified by the UriFormat property.
    /// </summary>
    public partial class MapImageLayer : MapPanel
    {
        public struct BoundingBox
        {
            public readonly double West;
            public readonly double East;
            public readonly double South;
            public readonly double North;

            public BoundingBox(double west, double east, double south, double north)
            {
                West = west;
                East = east;
                South = south;
                North = north;
            }
        }

        public static readonly DependencyProperty UriFormatProperty = DependencyProperty.Register(
            "UriFormat", typeof(string), typeof(MapImageLayer),
            new PropertyMetadata(null, (o, e) => ((MapImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty MinLongitudeProperty = DependencyProperty.Register(
            "MinLongitude", typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLongitudeProperty = DependencyProperty.Register(
            "MaxLongitude", typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MinLatitudeProperty = DependencyProperty.Register(
            "MinLatitude", typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLatitudeProperty = DependencyProperty.Register(
            "MaxLatitude", typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            "RelativeImageSize", typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            "UpdateInterval", typeof(TimeSpan), typeof(MapImageLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.5), (o, e) => ((MapImageLayer)o).updateTimer.Interval = (TimeSpan)e.NewValue));

        private readonly DispatcherTimer updateTimer;
        private int currentImageIndex;
        private bool updateInProgress;

        public MapImageLayer()
        {
            Children.Add(new MapImage { Opacity = 0d });
            Children.Add(new MapImage { Opacity = 0d });

            updateTimer = new DispatcherTimer { Interval = UpdateInterval };
            updateTimer.Tick += (s, e) => UpdateImage();
        }

        /// <summary>
        /// The format string of the image request Uri. The format must contain
        /// {X} and {Y} format specifiers for the map width and height in pixels and either
        /// {w},{s},{e},{n} for a latitude/longitude bounding box (like EPSG:4326) or
        /// {W},{S},{E},{N} for a projected bounding box (e.g. in meters like EPSG:3857).
        /// </summary>
        public string UriFormat
        {
            get { return (string)GetValue(UriFormatProperty); }
            set { SetValue(UriFormatProperty, value); }
        }

        /// <summary>
        /// Optional minimum longitude value. Default is NaN.
        /// </summary>
        public double MinLongitude
        {
            get { return (double)GetValue(MinLongitudeProperty); }
            set { SetValue(MinLongitudeProperty, value); }
        }

        /// <summary>
        /// Optional maximum longitude value. Default is NaN.
        /// </summary>
        public double MaxLongitude
        {
            get { return (double)GetValue(MaxLongitudeProperty); }
            set { SetValue(MaxLongitudeProperty, value); }
        }

        /// <summary>
        /// Optional minimum latitude value. Default is NaN.
        /// </summary>
        public double MinLatitude
        {
            get { return (double)GetValue(MinLatitudeProperty); }
            set { SetValue(MinLatitudeProperty, value); }
        }

        /// <summary>
        /// Optional maximum latitude value. Default is NaN.
        /// </summary>
        public double MaxLatitude
        {
            get { return (double)GetValue(MaxLatitudeProperty); }
            set { SetValue(MaxLatitudeProperty, value); }
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

        /// <summary>
        /// Minimum time interval between images updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get { return (TimeSpan)GetValue(UpdateIntervalProperty); }
            set { SetValue(UpdateIntervalProperty, value); }
        }

        protected virtual BoundingBox ProjectBoundingBox(BoundingBox boundingBox)
        {
            var p1 = ParentMap.MapTransform.Transform(new Location(boundingBox.South, boundingBox.West));
            var p2 = ParentMap.MapTransform.Transform(new Location(boundingBox.North, boundingBox.East));

            return new BoundingBox(
                TileSource.MetersPerDegree * p1.X, TileSource.MetersPerDegree * p2.X,
                TileSource.MetersPerDegree * p1.Y, TileSource.MetersPerDegree * p2.Y);
        }

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            updateTimer.Stop();
            updateTimer.Start();
        }

        protected void UpdateImage()
        {
            updateTimer.Stop();

            if (updateInProgress)
            {
                updateTimer.Start(); // update image on next timer tick
            }
            else if (ParentMap != null && ParentMap.RenderSize.Width > 0 && ParentMap.RenderSize.Height > 0)
            {
                updateInProgress = true;

                var relativeSize = Math.Max(RelativeImageSize, 1d);
                var width = ParentMap.RenderSize.Width * relativeSize;
                var height = ParentMap.RenderSize.Height * relativeSize;
                var dx = (ParentMap.RenderSize.Width - width) / 2d;
                var dy = (ParentMap.RenderSize.Height - height) / 2d;

                var loc1 = ParentMap.ViewportPointToLocation(new Point(dx, dy));
                var loc2 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy));
                var loc3 = ParentMap.ViewportPointToLocation(new Point(dx, dy + height));
                var loc4 = ParentMap.ViewportPointToLocation(new Point(dx + width, dy + height));

                var west = Math.Min(loc1.Longitude, Math.Min(loc2.Longitude, Math.Min(loc3.Longitude, loc4.Longitude)));
                var east = Math.Max(loc1.Longitude, Math.Max(loc2.Longitude, Math.Max(loc3.Longitude, loc4.Longitude)));
                var south = Math.Min(loc1.Latitude, Math.Min(loc2.Latitude, Math.Min(loc3.Latitude, loc4.Latitude)));
                var north = Math.Max(loc1.Latitude, Math.Max(loc2.Latitude, Math.Max(loc3.Latitude, loc4.Latitude)));

                if (!double.IsNaN(MinLongitude) && west < MinLongitude)
                {
                    west = MinLongitude;
                }

                if (!double.IsNaN(MaxLongitude) && east > MaxLongitude)
                {
                    east = MaxLongitude;
                }

                if (!double.IsNaN(MinLatitude) && south < MinLatitude)
                {
                    south = MinLatitude;
                }

                if (!double.IsNaN(MaxLatitude) && north > MaxLatitude)
                {
                    north = MaxLatitude;
                }

                var p1 = ParentMap.MapTransform.Transform(new Location(south, west));
                var p2 = ParentMap.MapTransform.Transform(new Location(north, east));

                UpdateImage(new BoundingBox(west, east, south, north),
                    (int)Math.Round((p2.X - p1.X) * ParentMap.ViewportScale),
                    (int)Math.Round((p2.Y - p1.Y) * ParentMap.ViewportScale));
            }
        }

        protected virtual void UpdateImage(BoundingBox boundingBox, int width, int height)
        {
            if (UriFormat != null && width > 0 && height > 0)
            {
                var uri = UriFormat
                    .Replace("{X}", width.ToString())
                    .Replace("{Y}", height.ToString());

                if (uri.Contains("{W}") && uri.Contains("{E}") && uri.Contains("{S}") && uri.Contains("{N}"))
                {
                    var projectedBoundingBox = ProjectBoundingBox(boundingBox);

                    uri = uri
                        .Replace("{W}", projectedBoundingBox.West.ToString(CultureInfo.InvariantCulture))
                        .Replace("{S}", projectedBoundingBox.South.ToString(CultureInfo.InvariantCulture))
                        .Replace("{E}", projectedBoundingBox.East.ToString(CultureInfo.InvariantCulture))
                        .Replace("{N}", projectedBoundingBox.North.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    uri = uri
                        .Replace("{w}", boundingBox.West.ToString(CultureInfo.InvariantCulture))
                        .Replace("{s}", boundingBox.South.ToString(CultureInfo.InvariantCulture))
                        .Replace("{e}", boundingBox.East.ToString(CultureInfo.InvariantCulture))
                        .Replace("{n}", boundingBox.North.ToString(CultureInfo.InvariantCulture));
                }

                UpdateImage(boundingBox, new Uri(uri));
            }
            else
            {
                UpdateImage(boundingBox, (BitmapSource)null);
            }
        }

        private void SetTopImage(BoundingBox boundingBox, BitmapSource bitmap)
        {
            currentImageIndex = (currentImageIndex + 1) % 2;
            var topImage = (MapImage)Children[currentImageIndex];

            topImage.SetBoundingBox(boundingBox.West, boundingBox.East, boundingBox.South, boundingBox.North);
            topImage.Source = bitmap;
        }

        private void SwapImages()
        {
            var topImage = (MapImage)Children[currentImageIndex];
            var bottomImage = (MapImage)Children[(currentImageIndex + 1) % 2];

            Canvas.SetZIndex(topImage, 1);
            Canvas.SetZIndex(bottomImage, 0);

            if (topImage.Source != null)
            {
                var fadeAnimation = new DoubleAnimation
                {
                    From = 0d,
                    To = 1d,
                    Duration = Tile.FadeDuration,
                    FillBehavior = FillBehavior.Stop
                };

                fadeAnimation.Completed += (s, e) =>
                {
                    bottomImage.Opacity = 0d;
                    bottomImage.Source = null;
                };

                topImage.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
                topImage.Opacity = 1d;
            }
            else
            {
                topImage.Opacity = 0d;
                bottomImage.Opacity = 0d;
                bottomImage.Source = null;
            }

            updateInProgress = false;
        }
    }
}
