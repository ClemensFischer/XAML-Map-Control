using System.Collections.Generic;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace SampleApplication
{
    public class MenuButton : Button
    {
#if WINUI || UWP
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

        protected static ToggleMenuFlyoutItem CreateMenuItem(string text, object item, RoutedEventHandler click)
        {
            var menuItem = new ToggleMenuFlyoutItem { Text = text, Tag = item };
            menuItem.Click += click;
            return menuItem;
        }

        protected static MenuFlyoutSeparator CreateSeparator()
        {
            return new MenuFlyoutSeparator();
        }
#else
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

        protected MenuButton()
        {
            Click += (s, e) => ContextMenu.IsOpen = true;
        }
#endif
    }
}
