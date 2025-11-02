using System.Text.RegularExpressions;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Views
{
  public partial class TimerSettingsView : System.Windows.Controls.UserControl
    {
public TimerSettingsView()
        {
InitializeComponent();
     }

        // Allow only numeric input
private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
    // Regex to allow only digits
   Regex regex = new Regex("[^0-9]+");
 e.Handled = regex.IsMatch(e.Text);
        }
    }
}