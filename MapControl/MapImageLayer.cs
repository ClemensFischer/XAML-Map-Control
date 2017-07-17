// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Map image layer. Fills the entire viewport with a map image, e.g. provided by a Web Map Service (WMS).
    /// The image must be provided by the abstract UpdateImage(BoundingBox) method.
    /// </summary>
    public abstract partial class MapImageLayer : MapPanel, IMapLayer
    {
        public static readonly DependencyProperty MinLatitudeProperty = DependencyProperty.Register(
            nameof(MinLatitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLatitudeProperty = DependencyProperty.Register(
            nameof(MaxLatitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MinLongitudeProperty = DependencyProperty.Register(
            nameof(MinLongitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLongitudeProperty = DependencyProperty.Register(
            nameof(MaxLongitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxBoundingBoxWidthProperty = DependencyProperty.Register(
            nameof(MaxBoundingBoxWidth), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            nameof(RelativeImageSize), typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            nameof(UpdateInterval), typeof(TimeSpan), typeof(MapImageLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.2), (o, e) => ((MapImageLayer)o).updateTimer.Interval = (TimeSpan)e.NewValue));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            nameof(UpdateWhileViewportChanging), typeof(bool), typeof(MapImageLayer), new PropertyMetadata(false));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapImageLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

        private readonly DispatcherTimer updateTimer;
        private BoundingBox boundingBox;
        private int topImageIndex;
        private bool updateInProgress;

        public MapImageLayer()
        {
            Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });
            Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });

            updateTimer = new DispatcherTimer { Interval = UpdateInterval };
            updateTimer.Tick += (s, e) => UpdateImage();
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
        /// Optional maximum width of the map image's bounding box. Default is NaN.
        /// </summary>
        public double MaxBoundingBoxWidth
        {
            get { return (double)GetValue(MaxBoundingBoxWidthProperty); }
            set { SetValue(MaxBoundingBoxWidthProperty, value); }
        }

        /// <summary>
        /// Relative size of the map image in relation to the current viewport size.
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

        /// <summary>
        /// Controls if images are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get { return (bool)GetValue(UpdateWhileViewportChangingProperty); }
            set { SetValue(UpdateWhileViewportChangingProperty, value); }
        }

        /// <summary>
        /// Description of the MapImageLayer.
        /// Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Optional foreground brush.
        /// Sets MapBase.Foreground if not null and the MapImageLayer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get { return (Brush)GetValue(MapForegroundProperty); }
            set { SetValue(MapForegroundProperty, value); }
        }

        /// <summary>
        /// Optional background brush.
        /// Sets MapBase.Background if not null and the MapImageLayer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get { return (Brush)GetValue(MapBackgroundProperty); }
            set { SetValue(MapBackgroundProperty, value); }
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);

            if (e.ProjectionChanged)
            {
                UpdateImage((BitmapSource)null);
                UpdateImage();
            }
            else
            {
                if (Math.Abs(e.LongitudeOffset) > 180d && boundingBox != null && boundingBox.HasValidBounds)
                {
                    var offset = 360d * Math.Sign(e.LongitudeOffset);

                    boundingBox.West += offset;
                    boundingBox.East += offset;

                    foreach (UIElement element in Children)
                    {
                        var bbox = GetBoundingBox(element);

                        if (bbox != null && bbox.HasValidBounds)
                        {
                            SetBoundingBox(element, new BoundingBox(bbox.South, bbox.West + offset, bbox.North, bbox.East + offset));
                        }
                    }
                }

                if (updateTimer.IsEnabled && !UpdateWhileViewportChanging)
                {
                    updateTimer.Stop(); // restart
                }

                if (!updateTimer.IsEnabled)
                {
                    updateTimer.Start();
                }
            }
        }

        protected virtual void UpdateImage()
        {
            updateTimer.Stop();

            if (updateInProgress)
            {
                updateTimer.Start(); // update image on next timer tick
            }
            else if (ParentMap != null && ParentMap.RenderSize.Width > 0 && ParentMap.RenderSize.Height > 0)
            {
                updateInProgress = true;

                var width = ParentMap.RenderSize.Width * RelativeImageSize;
                var height = ParentMap.RenderSize.Height * RelativeImageSize;
                var x = (ParentMap.RenderSize.Width - width) / 2d;
                var y = (ParentMap.RenderSize.Height - height) / 2d;
                var rect = new Rect(x, y, width, height);

                boundingBox = ParentMap.MapProjection.ViewportRectToBoundingBox(rect);

                if (boundingBox != null && boundingBox.HasValidBounds)
                {
                    if (!double.IsNaN(MinLatitude) && boundingBox.South < MinLatitude)
                    {
                        boundingBox.South = MinLatitude;
                    }

                    if (!double.IsNaN(MinLongitude) && boundingBox.West < MinLongitude)
                    {
                        boundingBox.West = MinLongitude;
                    }

                    if (!double.IsNaN(MaxLatitude) && boundingBox.North > MaxLatitude)
                    {
                        boundingBox.North = MaxLatitude;
                    }

                    if (!double.IsNaN(MaxLongitude) && boundingBox.East > MaxLongitude)
                    {
                        boundingBox.East = MaxLongitude;
                    }

                    if (!double.IsNaN(MaxBoundingBoxWidth) && boundingBox.Width > MaxBoundingBoxWidth)
                    {
                        var d = (boundingBox.Width - MaxBoundingBoxWidth) / 2d;
                        boundingBox.West += d;
                        boundingBox.East -= d;
                    }
                }

                var imageUpdated = false;

                try
                {
                    imageUpdated = UpdateImage(boundingBox);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if (!imageUpdated)
                {
                    UpdateImage((BitmapSource)null);
                }
            }
        }

        /// <summary>
        /// Creates an image request Uri or a BitmapSource for the specified image bounding box.
        /// Must either call UpdateImage(Uri) or UpdateImage(BitmapSource) or return false on failure.
        /// </summary>
        protected abstract bool UpdateImage(BoundingBox boundingBox);

        private void SetTopImage(BitmapSource bitmap)
        {
            topImageIndex = (topImageIndex + 1) % 2;
            var topImage = (Image)Children[topImageIndex];

            topImage.Source = bitmap;
            SetBoundingBox(topImage, boundingBox?.Clone());
        }

        private void SwapImages()
        {
            var topImage = (Image)Children[topImageIndex];
            var bottomImage = (Image)Children[(topImageIndex + 1) % 2];

            Canvas.SetZIndex(topImage, 1);
            Canvas.SetZIndex(bottomImage, 0);

            if (topImage.Source != null)
            {
                topImage.BeginAnimation(OpacityProperty, new DoubleAnimation
                {
                    To = 1d,
                    Duration = Tile.FadeDuration
                });

                bottomImage.BeginAnimation(OpacityProperty, new DoubleAnimation
                {
                    To = 0d,
                    BeginTime = Tile.FadeDuration,
                    Duration = TimeSpan.Zero
                });
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
