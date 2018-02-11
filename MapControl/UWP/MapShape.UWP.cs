// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public abstract partial class MapShape : Path
    {
        private void ParentMapChanged()
        {
            UpdateData();
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateData();
        }

        protected void LocationsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            var newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= LocationCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += LocationCollectionChanged;
            }

            UpdateData();
        }

        protected void LocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected void CreatePolylineFigures(IList<Point> points)
        {
            var viewport = new Rect(0, 0, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height);
            var figures = ((PathGeometry)Data).Figures;

            PathFigure figure = null;
            PolyLineSegment segment = null;

            for (int i = 1; i < points.Count; i++)
            {
                var p1 = points[i - 1];
                var p2 = points[i];
                var inside = Intersections.GetIntersections(ref p1, ref p2, viewport);

                if (inside)
                {
                    if (figure == null)
                    {
                        figure = new PathFigure { StartPoint = p1, IsClosed = false, IsFilled = false };
                        segment = new PolyLineSegment();
                        figure.Segments.Add(segment);
                        figures.Add(figure);
                    }

                    segment.Points.Add(p2);
                }

                if (!inside || p2 != points[i])
                {
                    figure = null;
                }
            }
        }
    }
}
