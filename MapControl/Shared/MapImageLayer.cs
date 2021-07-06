// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a single map image, e.g. from a Web Map Service (WMS).
    /// The image must be provided by the abstract GetImageAsync() method.
    /// </summary>
    public abstract class MapImageLayer : MapPanel, IMapLayer
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapImageLayer), new PropertyMetadata(null));

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

        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

#if WINUI
        private readonly DispatcherQueueTimer updateTimer;
#else
        private readonly DispatcherTimer updateTimer = new DispatcherTimer();
#endif
        private bool updateInProgress;

        public MapImageLayer()
        {
            Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });
            Children.Add(new Image { Opacity = 0d, Stretch = Stretch.Fill });

#if WINUI
            updateTimer = DispatcherQueue.CreateTimer();
#endif
            updateTimer.Interval = UpdateInterval;
            updateTimer.Tick += async (s, e) => await UpdateImageAsync();
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
        /// Relative size of the map image in relation to the current view size.
        /// Setting a value greater than one will let MapImageLayer request images that
        /// are larger than the view, in order to support smooth panning.
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

        /// <summary>
        /// The current BoundingBox
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Returns an ImageSource for the current BoundingBox.
        /// </summary>
        protected abstract Task<ImageSource> GetImageAsync();

        protected override async void OnViewportChanged(ViewportChangedEventArgs e)
        {
            if (e.ProjectionChanged)
            {
                ClearImages();

                base.OnViewportChanged(e);

                await UpdateImageAsync();
            }
            else
            {
                AdjustBoundingBox(e.LongitudeOffset);

                base.OnViewportChanged(e);

                if (!UpdateWhileViewportChanging)
                {
                    updateTimer.Stop(); // restart
                }

                updateTimer.Start();
            }
        }

        protected async Task UpdateImageAsync()
        {
            updateTimer.Stop();

            if (updateInProgress)
            {
                updateTimer.Start(); // update image on next timer tick
            }
            else if (ParentMap != null && ParentMap.RenderSize.Width > 0 && ParentMap.RenderSize.Height > 0)
            {
                updateInProgress = true;

                UpdateBoundingBox();

                ImageSource image = null;

                if (BoundingBox != null)
                {
                    try
                    {
                        image = await GetImageAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MapImageLayer: {ex.Message}");
                    }
                }

                SwapImages(image);

                updateInProgress = false;
            }
        }

        private void UpdateBoundingBox()
        {
            var width = ParentMap.RenderSize.Width * RelativeImageSize;
            var height = ParentMap.RenderSize.Height * RelativeImageSize;
            var x = (ParentMap.RenderSize.Width - width) / 2d;
            var y = (ParentMap.RenderSize.Height - height) / 2d;
            var rect = new Rect(x, y, width, height);

            BoundingBox = ParentMap.ViewRectToBoundingBox(rect);

            if (BoundingBox != null)
            {
                if (!double.IsNaN(MinLatitude) && BoundingBox.South < MinLatitude)
                {
                    BoundingBox.South = MinLatitude;
                }

                if (!double.IsNaN(MinLongitude) && BoundingBox.West < MinLongitude)
                {
                    BoundingBox.West = MinLongitude;
                }

                if (!double.IsNaN(MaxLatitude) && BoundingBox.North > MaxLatitude)
                {
                    BoundingBox.North = MaxLatitude;
                }

                if (!double.IsNaN(MaxLongitude) && BoundingBox.East > MaxLongitude)
                {
                    BoundingBox.East = MaxLongitude;
                }

                if (!double.IsNaN(MaxBoundingBoxWidth) && BoundingBox.Width > MaxBoundingBoxWidth)
                {
                    var d = (BoundingBox.Width - MaxBoundingBoxWidth) / 2d;
                    BoundingBox.West += d;
                    BoundingBox.East -= d;
                }
            }
        }

        private void AdjustBoundingBox(double longitudeOffset)
        {
            if (Math.Abs(longitudeOffset) > 180d && BoundingBox != null)
            {
                var offset = 360d * Math.Sign(longitudeOffset);

                BoundingBox.West += offset;
                BoundingBox.East += offset;

                foreach (var element in Children.OfType<FrameworkElement>())
                {
                    var bbox = GetBoundingBox(element);

                    if (bbox != null)
                    {
                        SetBoundingBox(element, new BoundingBox(bbox.South, bbox.West + offset, bbox.North, bbox.East + offset));
                    }
                }
            }
        }

        private void ClearImages()
        {
            foreach (var element in Children.OfType<FrameworkElement>())
            {
                element.ClearValue(BoundingBoxProperty);
                element.ClearValue(Image.SourceProperty);
            }
        }

        private void SwapImages(ImageSource image)
        {
            var topImage = (Image)Children[0];
            var bottomImage = (Image)Children[1];

            Children.RemoveAt(0);
            Children.Insert(1, topImage);

            topImage.Source = image;
            SetBoundingBox(topImage, BoundingBox?.Clone());

            topImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            });

            bottomImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            });
        }
    }
}
