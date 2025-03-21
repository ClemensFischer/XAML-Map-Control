using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MapControl.UiTools
{
    [ContentProperty(nameof(Items))]
    public partial class MenuButton
    {
        static MenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButton), new FrameworkPropertyMetadata(typeof(MenuButton)));
        }

        public MenuButton()
        {
            ContextMenu = new ContextMenu();
            DataContextChanged += (s, e) => ContextMenu.DataContext = e.NewValue;
            Loaded += async (s, e) => await Initialize();
            Click += (s, e) => ContextMenu.IsOpen = true;
        }

        public string Icon
        {
            get => Content as string;
            set => Content = value;
        }

        public ContextMenu Menu => ContextMenu;

        public ItemCollection Items => ContextMenu.Items;
    }
}
