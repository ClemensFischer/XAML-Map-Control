// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MapControl.UiTools
{
    public class MenuButton : Button
    {
        static MenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButton), new FrameworkPropertyMetadata(typeof(MenuButton)));
        }

        protected MenuButton(string icon)
        {
            Content = icon;

            Click += (s, e) => ContextMenu.IsOpen = true;
        }

        protected ContextMenu CreateMenu()
        {
            var menu = new ContextMenu();
            ContextMenu = menu;
            return menu;
        }

        protected IEnumerable<MenuItem> GetMenuItems()
        {
            return ContextMenu.Items.OfType<MenuItem>();
        }

        protected static MenuItem CreateMenuItem(string text, object item, RoutedEventHandler click)
        {
            var menuItem = new MenuItem { Header = text, Tag = item };
            menuItem.Click += click;
            return menuItem;
        }

        protected static Separator CreateSeparator()
        {
            return new Separator();
        }
    }
}
