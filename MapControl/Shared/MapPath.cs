// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for map shapes. The shape geometry is given by the Data property,
    /// which must contain a Geometry defined in cartesian (projected) map coordinates.
    /// Optionally, the Location property can by set to adjust the viewport position to the
    /// visible map viewport, as done for elements where the MapPanel.Location property is set.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapPath),
            new PropertyMetadata(null, (o, e) => ((MapPath)o).LocationPropertyChanged()));

        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        private readonly TransformGroup viewportTransform = new TransformGroup();
        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                if (parentMap != null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                viewportTransform.Children.Clear();
                parentMap = value;

                if (parentMap != null)
                {
                    viewportTransform.Children.Add(new TranslateTransform());
                    viewportTransform.Children.Add(parentMap.MapProjection.ViewportTransform);
                    parentMap.ViewportChanged += OnViewportChanged;
                }

                UpdateData();
            }
        }

        protected virtual void UpdateData()
        {
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
        {
            double longitudeScale = parentMap.MapProjection.LongitudeScale;

            if (e.ProjectionChanged)
            {
                viewportTransform.Children[1] = parentMap.MapProjection.ViewportTransform;
            }

            if (e.ProjectionChanged || double.IsNaN(longitudeScale))
            {
                UpdateData();
            }

            if (!double.IsNaN(longitudeScale)) // a normal cylindrical projection
            {
                var longitudeOffset = 0d;

                if (Location != null)
                {
                    var viewportPosition = parentMap.MapProjection.LocationToViewportPoint(Location);

                    if (viewportPosition.X < 0d || viewportPosition.X > parentMap.RenderSize.Width ||
                        viewportPosition.Y < 0d || viewportPosition.Y > parentMap.RenderSize.Height)
                    {
                        var nearestLongitude = Location.NearestLongitude(Location.Longitude, parentMap.Center.Longitude);

                        longitudeOffset = longitudeScale * (nearestLongitude - Location.Longitude);
                    }
                }

                ((TranslateTransform)viewportTransform.Children[0]).X = longitudeOffset;
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            OnViewportChanged(e);
        }

        private void LocationPropertyChanged()
        {
            if (parentMap != null)
            {
                OnViewportChanged(new ViewportChangedEventArgs());
            }
        }
    }
}
