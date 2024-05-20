// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    public static class DependencyPropertyHelper
    {
        public static DependencyProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool bindTwoWayByDefault = false,
            Action<TOwner, TValue, TValue> propertyChanged = null)
            where TOwner : DependencyObject
        {
#if WINUI || UWP
            var metadata = propertyChanged != null
                ? new PropertyMetadata(defaultValue, (o, e) => propertyChanged((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue))
                : new PropertyMetadata(defaultValue);

#else
            var metadata = new FrameworkPropertyMetadata
            {
                DefaultValue = defaultValue,
                BindsTwoWayByDefault = bindTwoWayByDefault
            };

            if (propertyChanged != null)
            {
                metadata.PropertyChangedCallback = (o, e) => propertyChanged((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue);
            }
#endif
            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), metadata);
        }

        public static DependencyProperty RegisterAttached<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool inherits = false,
            Action<FrameworkElement, TValue, TValue> propertyChanged = null)
            where TOwner : DependencyObject
        {
#if WINUI || UWP
            var metadata = propertyChanged != null
                ? new PropertyMetadata(defaultValue, (o, e) => propertyChanged((FrameworkElement)o, (TValue)e.OldValue, (TValue)e.NewValue))
                : new PropertyMetadata(defaultValue);
#else
            var metadata = new FrameworkPropertyMetadata
            {
                DefaultValue = defaultValue,
                Inherits = inherits
            };

            if (propertyChanged != null)
            {
                metadata.PropertyChangedCallback = (o, e) => propertyChanged((FrameworkElement)o, (TValue)e.OldValue, (TValue)e.NewValue);
            }
#endif
            return DependencyProperty.RegisterAttached(name, typeof(TValue), typeof(TOwner), metadata);
        }
    }
}
