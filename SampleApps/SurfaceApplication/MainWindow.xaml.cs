using System.Windows.Input;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace SurfaceApplication
{
    public partial class MainWindow : SurfaceWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MapTouchDown(object sender, TouchEventArgs e)
        {
            if (SurfaceEnvironment.IsSurfaceEnvironmentAvailable &&
                !e.Device.GetIsFingerRecognized())
            {
                // If touch event is from a blob or tag, prevent touch capture by setting
                // TouchEventArgs.Handled = true. Hence no manipulation will be started.
                // See http://msdn.microsoft.com/en-us/library/ms754010#touch_and_manipulation

                e.Handled = true;
            }
        }
    }
}