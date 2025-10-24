using System;

namespace GroupThree.FocusTimerApp.Models
{
    public class RegisteredAppModel
    {
        // 🧱 Thông tin cơ bản của ứng dụng
        public string AppName { get; set; } = string.Empty;         // Tên process (vd: Chrome, Word)
        public string ExecutablePath { get; set; } = string.Empty;  // Đường dẫn .exe
        public DateTime LastActive { get; set; }                    // Lần cuối được focus
        public bool IsRunning { get; set; }                         // Có đang chạy không

        // 🧠 (Tuỳ chọn) Nếu bạn có cờ khác, thêm ở đây
        public bool IsRegistered { get; set; }
    }
}
