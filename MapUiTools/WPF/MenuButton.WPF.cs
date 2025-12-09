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
            DataContextChanged += (_, e) => ContextMenu.DataContext = e.NewValue;
            Loaded += async (_, _) => await Initialize();
            Click += (_, _) => ContextMenu.IsOpen = true;
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
