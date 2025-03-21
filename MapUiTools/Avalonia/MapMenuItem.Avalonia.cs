using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl.UiTools
{
    public class MapMenuItem : MenuItem
    {
        public MapMenuItem()
        {
            Icon = new TextBlock
            {
                FontFamily = new("Segoe MDL2 Assets"),
                FontWeight = FontWeight.Black,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => (Parent as ItemsControl)?.Items.OfType<MapMenuItem>();

        protected override Type StyleKeyOverride => typeof(MenuItem);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property == IsCheckedProperty)
            {
                ((TextBlock)Icon).Text = (bool)args.NewValue ? "\uE73E" : ""; // CheckMark
            }
        }
    }
}
