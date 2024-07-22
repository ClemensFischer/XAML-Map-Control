// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class ToggleMenuFlyoutItem : MenuItem
    {
        internal static readonly FontFamily SymbolFont = new("Segoe MDL2 Assets");

        private readonly StackPanel header;
        private readonly TextBlock icon;

        public ToggleMenuFlyoutItem(string text, object item, EventHandler<RoutedEventArgs> click)
        {
            icon = new TextBlock
            {
                FontFamily = SymbolFont,
                Width = 20,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            header = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
            header.Children.Add(icon);
            header.Children.Add(new TextBlock { Text = text });

            Header = header;
            Tag = item;

            Click += click;
        }

        protected override Type StyleKeyOverride => typeof(MenuItem);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property == IsCheckedProperty)
            {
                icon.Text = (bool)args.NewValue ? "\uE73E" : ""; // CheckMark
            }
        }
    }

    public class MenuButton : Button
    {
        protected MenuButton(string icon)
        {
            var style = new Style();
            style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, ToggleMenuFlyoutItem.SymbolFont));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 20d));
            style.Setters.Add(new Setter(PaddingProperty, new Thickness(8)));
            Styles.Add(style);

            Content = icon;
        }

        protected override Type StyleKeyOverride => typeof(Button);

        protected MenuFlyout CreateMenu()
        {
            var menu = new MenuFlyout();
            Flyout = menu;
            return menu;
        }

        protected IEnumerable<ToggleMenuFlyoutItem> GetMenuItems()
        {
            return ((MenuFlyout)Flyout).Items.OfType<ToggleMenuFlyoutItem>();
        }

        protected static MenuItem CreateMenuItem(string text, object item, EventHandler<RoutedEventArgs> click)
        {
            return new ToggleMenuFlyoutItem(text, item, click);
        }

        protected static Separator CreateSeparator()
        {
            return new Separator();
        }
    }
}
