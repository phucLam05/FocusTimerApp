using System;
using System.Drawing;
using System.Media;
// 👇 Đặt alias cho namespace Forms
using Forms = System.Windows.Forms;

namespace GroupThree.FocusTimerApp.Services
{
    public static class NotificationService
    {
        private static Forms.NotifyIcon? _notifyIcon;
        private static SettingsService? _settingsService;

        public static void Initialize(SettingsService settingsService)
        {
            _settingsService = settingsService;

            if (_notifyIcon is null)
            {
                _notifyIcon = new Forms.NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Information,
                    Visible = true,
                    Text = "Focus Timer"
                };
            }

            AppDomain.CurrentDomain.ProcessExit += (_, _) => _notifyIcon?.Dispose();
        }

        public static void ShowNotification(string title, string message)
        {
            try
            {
                var cfg = _settingsService?.LoadSettings();
                if (cfg?.Notification?.EnableNotifications == false)
                    return;

                _notifyIcon?.ShowBalloonTip(3000, title, message, Forms.ToolTipIcon.Info);

                if (cfg?.Notification?.EnableSound == true)
                    SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi thông báo: " + ex.Message);
            }
        }
    }
}
