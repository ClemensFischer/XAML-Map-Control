using System;
using System.Windows;

namespace MapControl
{
    public static class DependencyPropertyHelper
    {
        public static DependencyProperty RegisterAttached<TValue>(
            string name,
            Type ownerType,
            TValue defaultValue = default,
            Action<FrameworkElement, TValue, TValue> changed = null,
            bool inherits = false)
        {
            var metadata = new FrameworkPropertyMetadata
            {
                DefaultValue = defaultValue,
                Inherits = inherits
            };

            if (changed != null)
            {
                metadata.PropertyChangedCallback = (o, e) =>
                {
                    if (o is FrameworkElement element)
                    {
                        changed(element, (TValue)e.OldValue, (TValue)e.NewValue);
                    }
                };
            }

            return DependencyProperty.RegisterAttached(name, typeof(TValue), ownerType, metadata);
        }

        public static DependencyProperty RegisterAttached<TValue>(
            string name,
            Type ownerType,
            TValue defaultValue,
            FrameworkPropertyMetadataOptions options)
        {
            var metadata = new FrameworkPropertyMetadata(defaultValue, options);

            return DependencyProperty.RegisterAttached(name, typeof(TValue), ownerType, metadata);
        }

        public static DependencyProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue,
            FrameworkPropertyMetadataOptions options)
            where TOwner : DependencyObject
        {
            var metadata = new FrameworkPropertyMetadata(defaultValue, options);

            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), metadata);
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

        public static DependencyPropertyKey RegisterReadOnly<TOwner, TValue>(
            string name,
            TValue defaultValue = default)
            where TOwner : DependencyObject
        {
            return DependencyProperty.RegisterReadOnly(name, typeof(TValue), typeof(TOwner), new PropertyMetadata(defaultValue));
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            DependencyProperty source) where TOwner : DependencyObject
        {
            return source.AddOwner(typeof(TOwner));
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            DependencyProperty source,
            TValue defaultValue) where TOwner : DependencyObject
        {
            return source.AddOwner(typeof(TOwner), new FrameworkPropertyMetadata(defaultValue));
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            DependencyProperty source,
            Action<TOwner, TValue, TValue> changed) where TOwner : DependencyObject
        {
            return source.AddOwner(typeof(TOwner), new FrameworkPropertyMetadata(
                (o, e) => changed((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue)));
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            string _, // for compatibility with WinUI/UWP DependencyPropertyHelper
            DependencyProperty source) where TOwner : DependencyObject
        {
            return AddOwner<TOwner, TValue>(source);
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            string _, // for compatibility with WinUI/UWP DependencyPropertyHelper
            DependencyProperty source,
            Action<TOwner, TValue, TValue> changed) where TOwner : DependencyObject
        {
            return AddOwner(source, changed);
        }
    }
}
