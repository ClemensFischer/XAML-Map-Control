// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    public partial class Pushpin
    {
        static Pushpin()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Pushpin), new FrameworkPropertyMetadata(typeof(Pushpin)));
        }
    }
}
