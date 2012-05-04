// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;

namespace MapControl
{
    internal interface INotifyParentMapChanged
    {
        void ParentMapChanged(Map oldParentMap, Map newParentMap);
    }

    /// <summary>
    /// Base class for child elements of a MapPanel.
    /// </summary>
    public abstract class MapElement : FrameworkElement, INotifyParentMapChanged
    {
        protected MapElement()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        public Map ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
        }

        protected abstract void OnViewTransformChanged(Map parentMap);

        private void OnViewTransformChanged(object sender, EventArgs eventArgs)
        {
            OnViewTransformChanged((Map)sender);
        }

        void INotifyParentMapChanged.ParentMapChanged(Map oldParentMap, Map newParentMap)
        {
            if (oldParentMap != null)
            {
                oldParentMap.ViewTransformChanged -= OnViewTransformChanged;
            }

            if (newParentMap != null)
            {
                newParentMap.ViewTransformChanged += OnViewTransformChanged;
            }
        }
    }
}
