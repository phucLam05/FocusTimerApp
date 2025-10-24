using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class TimerSettingsView : UserControl
    {
        private static readonly Regex _numericRegex = new Regex("[^0-9]+");

        public TimerSettingsView()
        {
            InitializeComponent();
            
            // Debug: Check DataContext
            this.Loaded += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettingsView] Loaded - DataContext: {this.DataContext?.GetType().Name ?? "NULL"}");
                
                if (this.DataContext is TimerSettingsViewModel vm)
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettingsView] ViewModel found - WorkDuration: {vm.WorkDuration}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettingsView] ERROR: DataContext is NOT TimerSettingsViewModel!");
                }
            };
            
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is TimerSettingsViewModel vm)
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettingsView] DataContext changed - WorkDuration: {vm.WorkDuration}");
                }
            };
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Block non-numeric input immediately (no popup, just prevent typing)
            e.Handled = _numericRegex.IsMatch(e.Text);
        }
    }
}