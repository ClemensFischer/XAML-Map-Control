// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public abstract partial class MapShape : Shape, IWeakEventListener
    {
        public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
            nameof(FillRule), typeof(FillRule), typeof(MapShape),
            new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender,
                (o, e) => ((MapShape)o).Data.FillRule = (FillRule)e.NewValue));

        /// <summary>
        /// Gets or sets the FillRule of the StreamGeometry that represents the polyline.
        /// </summary>
        public FillRule FillRule
        {
            get { return (FillRule)GetValue(FillRuleProperty); }
            set { SetValue(FillRuleProperty, value); }
        }

        protected PathGeometry Data { get; }

        protected override Geometry DefiningGeometry
        {
            get { return Data; }
        }

        private void ParentMapChanged()
        {
            if (parentMap != null)
            {
                var transform = new TransformGroup();
                transform.Children.Add(new TranslateTransform(GetLongitudeOffset() * parentMap.MapProjection.TrueScale, 0d));
                transform.Children.Add(parentMap.MapProjection.ViewportTransform);

                Data.Transform = transform;
            }
            else
            {
                Data.Transform = Transform.Identity;
            }

            UpdateData();
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            var transform = (TransformGroup)Data.Transform;
            var offset = (TranslateTransform)transform.Children[0];

            offset.X = GetLongitudeOffset() * parentMap.MapProjection.TrueScale;

            if (e.ProjectionChanged)
            {
                transform.Children[1] = parentMap.MapProjection.ViewportTransform;
            }

            if (e.ProjectionChanged || parentMap.MapProjection.IsAzimuthal)
            {
                UpdateData();
            }
            else if (Fill != null)
            {
                InvalidateVisual(); // Fill brush may be rendered only partially or not at all
            }
        }

        protected void DataCollectionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            INotifyCollectionChanged locations;

            if ((locations = e.OldValue as INotifyCollectionChanged) != null)
            {
                CollectionChangedEventManager.RemoveListener(locations, this);
            }

            if ((locations = e.NewValue as INotifyCollectionChanged) != null)
            {
                CollectionChangedEventManager.AddListener(locations, this);
            }

            UpdateData();
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            UpdateData();

            return true;
        }
    }
}
