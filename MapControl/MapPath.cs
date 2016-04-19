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
    /// The Stretch property is meaningless for MapPath, it will be reset to None.
    /// Optionally, the MapPanel.Location property can by set to move the viewport position
    /// to a longitude with minimal distance to the longitude of the current map center.
    /// </summary>
    public partial class MapPath : IMapShape
    {
        private readonly TransformGroup viewportTransform = new TransformGroup();
        private MapBase parentMap;

        public TransformGroup ViewportTransform
        {
            get { return viewportTransform; }
        }

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                var location = MapPanel.GetLocation(this);

                if (parentMap != null && location != null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                viewportTransform.Children.Clear();
                parentMap = value;

                if (parentMap != null)
                {
                    viewportTransform.Children.Add(parentMap.ViewportTransform);

                    if (location != null)
                    {
                        viewportTransform.Children.Insert(0, new TranslateTransform());
                        parentMap.ViewportChanged += OnViewportChanged;
                        OnViewportChanged(this, EventArgs.Empty);
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

        void IMapShape.LocationChanged(Location oldValue, Location newValue)
        {
            if (parentMap != null)
            {
                if (newValue == null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                    viewportTransform.Children.RemoveAt(0);
                }
                else if (oldValue == null)
                {
                    viewportTransform.Children.Insert(0, new TranslateTransform());
                    parentMap.ViewportChanged += OnViewportChanged;
                    OnViewportChanged(this, EventArgs.Empty);
                }
            }
        }

        private void OnViewportChanged(object sender, EventArgs e)
        {
            var longitude = Location.NormalizeLongitude(MapPanel.GetLocation(this).Longitude);

            ((TranslateTransform)viewportTransform.Children[0]).X =
                Location.NearestLongitude(longitude, parentMap.Center.Longitude) - longitude;
        }
    }
}
