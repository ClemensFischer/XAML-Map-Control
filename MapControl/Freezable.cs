// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    internal static class Freezable
    {
        /// <summary>
        /// Provides WPF compatibility.
        /// </summary>
        public static void Freeze(this DependencyObject obj)
        {
        }
    }
}
