using System.Collections.Generic;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
#elif WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
#endif

namespace MapControl.UiTools
{
    [ContentProperty(Name = nameof(Items))]
    public partial class MenuButton
    {
        public MenuButton()
        {
            Flyout = new MenuFlyout();
            Loaded += async (_, _) => await Initialize();
        }

        public string Icon
        {
            get => (Content as FontIcon)?.Glyph;
            set => Content = new FontIcon { Glyph = value };
        }

        public MenuFlyout Menu => (MenuFlyout)Flyout;

        public IList<MenuFlyoutItemBase> Items => Menu.Items;
    }
}
