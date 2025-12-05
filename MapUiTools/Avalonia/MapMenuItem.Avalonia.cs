using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl.UiTools
{
    public abstract partial class MapMenuItem : MenuItem
    {
        protected MapMenuItem()
        {
            Icon = new TextBlock
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontWeight = FontWeight.Black,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            Loaded += (s, e) => Initialize();
            Click += (s, e) => Execute();
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();

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
