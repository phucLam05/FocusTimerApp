using System.Windows;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();

            this.Loaded += OverlayWindow_Loaded;

            this.MouseLeftButtonDown += OverlayWindow_MouseLeftButtonDown;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;

            var margin = 20.0;

            this.Left = workArea.Right - this.Width - margin;
            this.Top = workArea.Top + margin;
        }

        private void OverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}