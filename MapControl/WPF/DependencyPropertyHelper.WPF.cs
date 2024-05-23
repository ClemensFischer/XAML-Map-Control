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
            TValue defaultValue,
            FrameworkPropertyMetadataOptions options)
            where TOwner : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), new FrameworkPropertyMetadata(defaultValue, options));
        }

        public static DependencyProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            Action<TOwner, TValue, TValue> changed = null,
            Func<TOwner, TValue, TValue> coerce = null,
            bool bindTwoWayByDefault = false)
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
            Action<FrameworkElement, TValue, TValue> changed = null,
            bool inherits = false)
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

        public static DependencyProperty AddOwner<TOwner>(
            DependencyProperty property,
            FrameworkPropertyMetadataOptions options = FrameworkPropertyMetadataOptions.None)
            where TOwner : DependencyObject
        {
            FrameworkPropertyMetadata metadata = null;

            if (options != FrameworkPropertyMetadataOptions.None)
            {
                metadata = new FrameworkPropertyMetadata(property.DefaultMetadata.DefaultValue, options);
            }

            return property.AddOwner(typeof(TOwner), metadata);
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            DependencyProperty property,
            Action<TOwner, TValue, TValue> changed)
            where TOwner : DependencyObject
        {
            FrameworkPropertyMetadata metadata = null;

            if (changed != null)
            {
                metadata = new FrameworkPropertyMetadata((o, e) => changed((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue));
            }

            return property.AddOwner(typeof(TOwner), metadata);
        }
    }
}
