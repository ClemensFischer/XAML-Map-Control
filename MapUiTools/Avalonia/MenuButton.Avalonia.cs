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

        private readonly TextBlock icon = new()
        {
            FontFamily = SymbolFont,
            FontWeight = FontWeight.Black,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        public ToggleMenuFlyoutItem(string text, object item, EventHandler<RoutedEventArgs> click)
        {
            Icon = icon;
            Header = text;
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
