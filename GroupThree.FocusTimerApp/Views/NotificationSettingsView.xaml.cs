using System.Windows.Controls;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class NotificationSettingsView : UserControl
    {
        public NotificationSettingsView()
        {
            InitializeComponent();
            
            // Ensure DataContext is properly inherited
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is NotificationSettingsViewModel vm)
                {
                    System.Diagnostics.Debug.WriteLine($"[NotificationSettingsView] DataContext set - EnableNotifications: {vm.EnableNotifications}");
                }
            };
        }
    }
}