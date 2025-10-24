using System.Windows.Controls;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class TimerSettingsView : UserControl
    {
        public TimerSettingsView()
        {
            InitializeComponent();
            
            // Ensure DataContext is properly inherited
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is TimerSettingsViewModel vm)
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettingsView] DataContext set - WorkDuration: {vm.WorkDuration}");
                }
            };
        }
    }
}