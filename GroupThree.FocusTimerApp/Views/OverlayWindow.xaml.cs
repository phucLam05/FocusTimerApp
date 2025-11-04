using System.Windows;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Views
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();

            // 1. Đăng ký sự kiện Loaded để thiết lập vị trí ban đầu
            this.Loaded += OverlayWindow_Loaded;

            // 2. Đăng ký sự kiện MouseLeftButtonDown để cho phép kéo thả
            this.MouseLeftButtonDown += OverlayWindow_MouseLeftButtonDown;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // === TÍNH NĂNG 1: ĐẶT VỊ TRÍ GÓC PHẢI TRÊN ===

            // Lấy khu vực làm việc của màn hình (không bao gồm thanh taskbar)
            var workArea = SystemParameters.WorkArea;

            // Đặt một khoảng lề (margin) 20 pixel
            var margin = 20.0;

            // Tính toán và thiết lập vị trí Top và Left của cửa sổ
            // workArea.Right là cạnh phải của màn hình
            // workArea.Top là cạnh trên (thường là 0, nhưng an toàn hơn nếu dùng)
            this.Left = workArea.Right - this.Width - margin;
            this.Top = workArea.Top + margin;
        }

        private void OverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // === TÍNH NĂNG 2: CHO PHÉP KÉO THẢ CỬA SỔ ===

            // Vì cửa sổ của bạn không có viền (WindowStyle="None"),
            // chúng ta cần gọi DragMove() để Windows xử lý việc di chuyển.
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}