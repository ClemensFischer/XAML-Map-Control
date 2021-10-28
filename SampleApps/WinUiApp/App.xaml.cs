using Microsoft.UI.Xaml;

namespace SampleApplication
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            window = new MainWindow();
            window.Activate();
        }

        private Window window;
    }
}
