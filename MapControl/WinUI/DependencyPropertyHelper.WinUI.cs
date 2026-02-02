using System;
#if UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace MapControl
{
    public static class DependencyPropertyHelper
    {
        public static DependencyProperty RegisterAttached<TValue>(
            string name,
            Type ownerType,
            TValue defaultValue = default,
            Action<FrameworkElement, TValue, TValue> changed = null)
        {
            var metadata = changed == null
                ? new PropertyMetadata(defaultValue)
                : new PropertyMetadata(defaultValue, (o, e) =>
                {
                    if (o is FrameworkElement element)
                    {
                        changed(element, (TValue)e.OldValue, (TValue)e.NewValue);
                    }
                });

            return DependencyProperty.RegisterAttached(name, typeof(TValue), ownerType, metadata);
        }

        public static DependencyProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            Action<TOwner, TValue, TValue> changed = null)
            where TOwner : DependencyObject
        {
            var metadata = changed != null
                ? new PropertyMetadata(defaultValue, (o, e) => changed((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue))
                : new PropertyMetadata(defaultValue);

            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), metadata);
        }

        public static DependencyProperty AddOwner<TOwner, TValue>(
            string name,
            DependencyProperty source,
            Action<TOwner, TValue, TValue> changed = null)
            where TOwner : DependencyObject
        {
            var metadata = new PropertyMetadata(default, (o, e) =>
            {
                o.SetValue(source, e.NewValue);
                changed?.Invoke((TOwner)o, (TValue)e.OldValue, (TValue)e.NewValue);
            });

            return DependencyProperty.Register(name, typeof(TValue), typeof(TOwner), metadata);
        }
    }
}
