// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using DispatcherTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;
#elif UWP
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

        public static readonly DependencyProperty LoadingProgressProperty = DependencyProperty.Register(
            nameof(LoadingProgress), typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        private readonly Progress<double> imageProgress;
        private readonly DispatcherTimer updateTimer;
        private bool updateInProgress;

        public MapImageLayer()
        {
            imageProgress = new Progress<double>(p => LoadingProgress = p);

            updateTimer = this.CreateTimer(UpdateInterval);
            updateTimer.Tick += async (s, e) => await UpdateImageAsync();
        }

        /// <summary>
        /// Description of the layer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// Relative size of the map image in relation to the current view size.
        /// Setting a value greater than one will let MapImageLayer request images that
        /// are larger than the view, in order to support smooth panning.
        /// </summary>
        public double RelativeImageSize
        {
            get => (double)GetValue(RelativeImageSizeProperty);
            set => SetValue(RelativeImageSizeProperty, value);
        }

        /// <summary>
        /// Minimum time interval between images updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get => (TimeSpan)GetValue(UpdateIntervalProperty);
            set => SetValue(UpdateIntervalProperty, value);
        }

        /// <summary>
        /// Controls if images are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get => (bool)GetValue(UpdateWhileViewportChangingProperty);
            set => SetValue(UpdateWhileViewportChangingProperty, value);
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get => (Brush)GetValue(MapBackgroundProperty);
            set => SetValue(MapBackgroundProperty, value);
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get => (Brush)GetValue(MapForegroundProperty);
            set => SetValue(MapForegroundProperty, value);
        }

        /// <summary>
        /// Gets the progress of the ImageLoader as a double value between 0 and 1.
        /// </summary>
        public double LoadingProgress
        {
            get => (double)GetValue(LoadingProgressProperty);
            private set => SetValue(LoadingProgressProperty, value);
        }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
                while (Children.Count < 2)
                {
                    Children.Add(new Image
                    {
                        Opacity = 0d,
                        Stretch = Stretch.Fill,
                        IsHitTestVisible = false // avoid touch capture issues
                    });
                }
            }
            else
            {
                updateTimer.Stop();
                ClearImages();
                Children.Clear();
            }

            base.SetParentMap(map);
        }

        protected override async void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);

            if (e.ProjectionChanged)
            {
                ClearImages();

                await UpdateImageAsync(); // update immediately
            }
            else
            {
                updateTimer.Run(!UpdateWhileViewportChanging);
            }
        }

        protected abstract Task<ImageSource> GetImageAsync(BoundingBox boundingBox, IProgress<double> progress);

        protected async Task UpdateImageAsync()
        {
            if (updateInProgress)
            {
                // Update image on next tick, start timer if not running.
                //
                updateTimer.Run();
            }
            else
            {
                updateTimer.Stop();
                updateInProgress = true;

                ImageSource image = null;
                var boundingBox = GetImageBoundingBox();

                if (boundingBox != null)
                {
                    try
                    {
                        image = await GetImageAsync(boundingBox, imageProgress);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MapImageLayer: {ex.Message}");
                    }
                }

                SwapImages(image, boundingBox);

                updateInProgress = false;
            }
        }

        protected BoundingBox GetImageBoundingBox()
        {
            BoundingBox boundingBox = null;

            if (ParentMap != null && ParentMap.RenderSize.Width > 0d && ParentMap.RenderSize.Height > 0d)
            {
                var width = ParentMap.RenderSize.Width * RelativeImageSize;
                var height = ParentMap.RenderSize.Height * RelativeImageSize;
                var x = (ParentMap.RenderSize.Width - width) / 2d;
                var y = (ParentMap.RenderSize.Height - height) / 2d;

                boundingBox = ParentMap.ViewRectToBoundingBox(new Rect(x, y, width, height));
            }

            return boundingBox;
        }

        private void ClearImages()
        {
            foreach (var image in Children.OfType<Image>())
            {
                image.ClearValue(BoundingBoxProperty);
                image.ClearValue(Image.SourceProperty);
            }
        }

        private void SwapImages(ImageSource image, BoundingBox boundingBox)
        {
            if (Children.Count >= 2)
            {
                var topImage = (Image)Children[0];
                var bottomImage = (Image)Children[1];

                Children.RemoveAt(0);
                Children.Insert(1, topImage);

                topImage.Source = image;
                SetBoundingBox(topImage, boundingBox);

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
}
