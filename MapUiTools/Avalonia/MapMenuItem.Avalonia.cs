using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapControl.UiTools
{
    public abstract partial class MapMenuItem : MenuItem
    {
        protected MapMenuItem()
        {
            Icon = new TextBlock
            {
                FontFamily = new("Segoe MDL2 Assets"),
                FontWeight = FontWeight.Black,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            Loaded += (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    IsChecked = GetIsChecked(map);
                }
            };

            Click += async (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    await Execute(map);

                    foreach (var item in ParentMenuItems)
                    {
                        item.IsChecked = item.GetIsChecked(map);
                    }
                }
            };
        }

        public string Text
        {
            get => Header as string;
            set => Header = value;
        }

        protected IEnumerable<MapMenuItem> ParentMenuItems => ((ItemsControl)Parent).Items.OfType<MapMenuItem>();

        protected override Type StyleKeyOverride => typeof(MenuItem);

        protected abstract bool GetIsChecked(MapBase map);

        public abstract Task Execute(MapBase map);

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
