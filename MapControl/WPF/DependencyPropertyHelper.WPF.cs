// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;

namespace MapControl
{
    public static class DependencyPropertyHelper
    {
        public static DependencyProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool bindTwoWayByDefault = false,
            Action<TOwner, TValue, TValue> changed = null,
            Func<TOwner, TValue, TValue> coerce = null)
            where TOwner : DependencyObject
        {
            var metadata = new FrameworkPropertyMetadata
            {
                DefaultValue = defaultValue,
                BindsTwoWayByDefault = bindTwoWayByDefault
            };

            if (changed != null)
            {
                metadata.PropertyChangedCallback = (o, e) => changed((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue);
            }

            if (coerce != null)
            {
                metadata.CoerceValueCallback = (o, v) => coerce((TOwner)o, (TValue)v);
            }

            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), metadata);
        }

        public static DependencyProperty RegisterAttached<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool inherits = false,
            Action<FrameworkElement, TValue, TValue> changed = null)
            where TOwner : DependencyObject
        {
            var metadata = new FrameworkPropertyMetadata
            {
                DefaultValue = defaultValue,
                Inherits = inherits
            };

            if (changed != null)
            {
                metadata.PropertyChangedCallback = (o, e) => changed((FrameworkElement)o, (TValue)e.OldValue, (TValue)e.NewValue);
            }

            return DependencyProperty.RegisterAttached(name, typeof(TValue), typeof(TOwner), metadata);
        }

        public static DependencyPropertyKey RegisterReadOnly<TOwner, TValue>(
            string name,
            TValue defaultValue = default)
            where TOwner : DependencyObject
        {
            return DependencyProperty.RegisterReadOnly(name, typeof(TValue), typeof(TOwner), new PropertyMetadata(defaultValue));
        }
    }
}
