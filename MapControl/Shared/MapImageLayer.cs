using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a single map image, e.g. from a Web Map Service (WMS).
    /// The image must be provided by the abstract GetImageAsync() method.
    /// </summary>
    public abstract partial class MapImageLayer : MapPanel, IMapLayer
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyPropertyHelper.Register<MapImageLayer, string>(nameof(Description));

        public static readonly DependencyProperty RelativeImageSizeProperty =
            DependencyPropertyHelper.Register<MapImageLayer, double>(nameof(RelativeImageSize), 1d);

        public static readonly DependencyProperty UpdateIntervalProperty =
            DependencyPropertyHelper.Register<MapImageLayer, TimeSpan>(nameof(UpdateInterval), TimeSpan.FromSeconds(0.2),
                (layer, oldValue, newValue) => layer.updateTimer.Interval = newValue);

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty =
            DependencyPropertyHelper.Register<MapImageLayer, bool>(nameof(UpdateWhileViewportChanging));

        public static readonly DependencyProperty MapBackgroundProperty =
            DependencyPropertyHelper.Register<MapImageLayer, Brush>(nameof(MapBackground));

        public static readonly DependencyProperty MapForegroundProperty =
            DependencyPropertyHelper.Register<MapImageLayer, Brush>(nameof(MapForeground));

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyPropertyHelper.Register<MapImageLayer, double>(nameof(LoadingProgress), 1d);

        private readonly Progress<double> loadingProgress;
        private readonly DispatcherTimer updateTimer;
        private bool updateInProgress;

        public MapImageLayer()
        {
            IsHitTestVisible = false;

            loadingProgress = new Progress<double>(p => SetValue(LoadingProgressProperty, p));

            updateTimer = new DispatcherTimer { Interval = UpdateInterval };
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
        public double LoadingProgress => (double)GetValue(LoadingProgressProperty);

        public abstract IReadOnlyCollection<string> SupportedCrsIds { get; }

        protected override void SetParentMap(MapBase map)
        {
            if (map != null)
            {
                while (Children.Count < 2)
                {
                    Children.Add(new Image
                    {
                        Opacity = 0d,
                        Stretch = Stretch.Fill
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
                if (!UpdateWhileViewportChanging)
                {
                    updateTimer.Stop();
                }

                if (!updateTimer.IsEnabled)
                {
                    updateTimer.Start();
                }
            }
        }

        protected abstract Task<ImageSource> GetImageAsync(BoundingBox boundingBox, IProgress<double> progress);

        protected async Task UpdateImageAsync()
        {
            if (!updateInProgress)
            {
                updateInProgress = true;
                updateTimer.Stop();

                ImageSource image = null;
                BoundingBox boundingBox = null;

                if (ParentMap != null &&
                    ParentMap.ActualWidth > 0d &&
                    ParentMap.ActualHeight > 0d &&
                    (SupportedCrsIds == null || SupportedCrsIds.Contains(ParentMap.MapProjection.CrsId)))
                {
                    var width = ParentMap.ActualWidth * RelativeImageSize;
                    var height = ParentMap.ActualHeight * RelativeImageSize;
                    var x = (ParentMap.ActualWidth - width) / 2d;
                    var y = (ParentMap.ActualHeight - height) / 2d;

                    boundingBox = ParentMap.ViewRectToBoundingBox(new Rect(x, y, width, height));

                    image = await GetImageAsync(boundingBox, loadingProgress);
                }

                SwapImages(image, boundingBox);

                updateInProgress = false;
            }
            else if (!updateTimer.IsEnabled) // update on next timer tick
            {
                updateTimer.Start();
            }
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

                Children.RemoveAt(0);
                Children.Insert(1, topImage);

                topImage.Source = image;
                SetBoundingBox(topImage, boundingBox);

                if (MapBase.ImageFadeDuration > TimeSpan.Zero)
                {
                    FadeOver();
                }
                else
                {
                    topImage.Opacity = 1d;
                    Children[0].Opacity = 0d;
                }
            }
        }
    }
}
