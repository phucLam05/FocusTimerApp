using System.Windows;

namespace GroupThree.FocusTimerApp.Views
{
  public partial class SuccessDialog : Window
    {
        public SuccessDialog(string title, string message)
        {
      InitializeComponent();
      TitleText.Text = title;
 MessageText.Text = message;
  
  // Auto-focus OK button
     Loaded += (s, e) => OkButton.Focus();
      
      // Auto-close after 2 seconds
       var timer = new System.Windows.Threading.DispatcherTimer();
    timer.Interval = TimeSpan.FromSeconds(2);
       timer.Tick += (s, e) =>
  {
       timer.Stop();
    Close();
       };
       timer.Start();
 }

  private void OkButton_Click(object sender, RoutedEventArgs e)
   {
  Close();
        }
    }
}
