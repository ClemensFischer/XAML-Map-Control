// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
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
            "Location", typeof(Location), typeof(MapPath),
            new PropertyMetadata(null, (o, e) => ((MapPath)o).LocationChanged((Location)e.OldValue, (Location)e.NewValue)));

        private readonly TransformGroup viewportTransform = new TransformGroup();
        private MapBase parentMap;

        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public TransformGroup ViewportTransform
        {
            get { return viewportTransform; }
        }

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                if (parentMap != null && Location != null)
                {
                    DetachViewportChanged();
                }

                viewportTransform.Children.Clear();
                parentMap = value;

                if (parentMap != null)
                {
                    viewportTransform.Children.Add(parentMap.ViewportTransform);

                    if (Location != null)
                    {
                        AttachViewportChanged();
                    }
                }

                UpdateData();
            }
        }

        protected virtual void UpdateData()
        {
            if (Data != null)
            {
                Data.Transform = viewportTransform;
            }
        }

        private void LocationChanged(Location oldValue, Location newValue)
        {
            if (parentMap != null)
            {
                if (oldValue == null)
                {
                    AttachViewportChanged();
                }
                else if (newValue == null)
                {
                    DetachViewportChanged();
                }
            }
        }

        private void AttachViewportChanged()
        {
            viewportTransform.Children.Insert(0, new TranslateTransform());
            parentMap.ViewportChanged += OnViewportChanged;
            OnViewportChanged(parentMap, EventArgs.Empty);
        }

        private void DetachViewportChanged()
        {
            parentMap.ViewportChanged -= OnViewportChanged;
            viewportTransform.Children.RemoveAt(0);
        }

        private void OnViewportChanged(object sender, EventArgs e)
        {
            var viewportPosition = parentMap.LocationToViewportPoint(Location);
            var longitudeOffset = 0d;

            if (viewportPosition.X < 0d || viewportPosition.X > parentMap.RenderSize.Width ||
                viewportPosition.Y < 0d || viewportPosition.Y > parentMap.RenderSize.Height)
            {
                longitudeOffset = Location.NearestLongitude(Location.Longitude, parentMap.Center.Longitude) - Location.Longitude;
            }

            ((TranslateTransform)viewportTransform.Children[0]).X = longitudeOffset;
        }
    }
}
