using System.Windows;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class ConfirmDialog : Window
 {
   public bool Result { get; private set; }

        public ConfirmDialog(string title, string message)
        {
   InitializeComponent();
     TitleText.Text = title;
   MessageText.Text = message;
      
     // Focus Yes button when loaded
      Loaded += (s, e) => YesButton.Focus();
  }

      private void YesButton_Click(object sender, RoutedEventArgs e)
    {
   Result = true;
      DialogResult = true;
   Close();
  }

      private void NoButton_Click(object sender, RoutedEventArgs e)
  {
     Result = false;
       DialogResult = false;
  Close();
        }
    }
}
