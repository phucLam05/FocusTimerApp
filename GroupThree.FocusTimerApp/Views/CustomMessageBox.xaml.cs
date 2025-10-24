using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class CustomMessageBox : Window
    {
        public enum MessageType
        {
            Success,
            Error,
            Info,
            Warning
        }

        public CustomMessageBox(string message, string title, MessageType type = MessageType.Success)
        {
            InitializeComponent();
            
            MessageText.Text = message;
            TitleText.Text = title;
            
            Color iconColor;
            string iconData;
            
            switch (type)
            {
                case MessageType.Success:
                    iconData = "M12,2C6.48,2 2,6.48 2,12C2,17.52 6.48,22 12,22C17.52,22 22,17.52 22,12C22,6.48 17.52,2 12,2M10,17L5,12L6.41,10.59L10,14.17L17.59,6.58L19,8L10,17Z";
                    iconColor = Color.FromRgb(34, 197, 94);
                    break;
                    
                case MessageType.Error:
                    iconData = "M12,2C6.47,2 2,6.47 2,12C2,17.53 6.47,22 12,22C17.53,22 22,17.53 22,12C22,6.47 17.53,2 12,2M17,15.59L15.59,17L12,13.41L8.41,17L7,15.59L10.59,12L7,8.41L8.41,7L12,10.59L15.59,7L17,8.41L13.41,12L17,15.59Z";
                    iconColor = Color.FromRgb(239, 68, 68);
                    break;
                    
                case MessageType.Warning:
                    iconData = "M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z";
                    iconColor = Color.FromRgb(251, 191, 36);
                    break;
                    
                case MessageType.Info:
                default:
                    iconData = "M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
                    iconColor = Color.FromRgb(99, 102, 241);
                    break;
            }
            
            IconPath.Data = Geometry.Parse(iconData);
            IconPath.Fill = new SolidColorBrush(iconColor);
            IconBackground.Color = iconColor;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public static void Show(string message, string title = "Message", MessageType type = MessageType.Success)
        {
            var messageBox = new CustomMessageBox(message, title, type);
            messageBox.ShowDialog();
        }
    }
}
