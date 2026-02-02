#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// A path element with a Data property that holds a Geometry in view coordinates or
    /// projected map coordinates that are relative to an origin Location.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapPath, Location>(nameof(Location), null,
                (path, oldValue, newValue) => path.UpdateData());

        /// <summary>
        /// Gets or sets a Location that is either used as
        /// - the origin point of a geometry specified in projected map coordinates (meters) or
        /// - as an optional anchor point to constrain the view position of MapPaths with
        ///   multiple Locations (like MapPolyline or MapPolygon) to the visible map viewport,
        ///   as done for elements where the MapPanel.Location property is set.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get;
            set
            {
                if (field != null)
                {
                    field.ViewportChanged -= OnViewportChanged;
                }

                field = value;

                if (field != null)
                {
                    field.ViewportChanged += OnViewportChanged;
                }

                UpdateData();
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateData();
        }

        protected void SetDataTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform
#if WPF
                && !transform.IsFrozen
#endif
                )
            {
                transform.Matrix = matrix;
            }
            else
            {
                Data.Transform = new MatrixTransform { Matrix = matrix };
            }
        }

        protected virtual void UpdateData()
        {
            if (Data != null && ParentMap != null && Location != null)
            {
                SetDataTransform(ParentMap.GetMapToViewTransform(Location));
            }

            MapPanel.SetLocation(this, Location);
        }

        protected Point LocationToMap(Location location, double longitudeOffset)
        {
            var point = ParentMap.MapProjection.LocationToMap(location.Latitude, location.Longitude + longitudeOffset);

            if (point.Y == double.PositiveInfinity)
            {
                point = new Point(point.X, 1e9);
            }
            else if (point.Y == double.NegativeInfinity)
            {
                point = new Point(point.X, -1e9);
            }

            return point;
        }

        protected Point LocationToView(Location location, double longitudeOffset)
        {
            return ParentMap.ViewTransform.MapToView(LocationToMap(location, longitudeOffset));
        }
    }
}
