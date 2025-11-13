namespace GroupThree.FocusTimerApp.Services
{
    public static class NotificationService
    {
        private static SettingsService? _settingsService;

        // Initialize with SettingsService (call this once at app startup)
        public static void Initialize(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public static void Show(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            // Check if notifications are enabled
            if (_settingsService != null)
            {
                try
                {
                    var cfg = _settingsService.LoadSettings();
                    if (cfg.Notification?.EnableNotifications != true)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NotificationService] Notification blocked: {title} - {text}");
                        return; // Notifications disabled, don't show
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NotificationService] Error checking settings: {ex.Message}");
                    return;
                }
            }

            try
            {
                var notify = new NotifyIcon
                {
                    Icon = Properties.Resources.AppIcon,
                    Visible = true,
                    BalloonTipTitle = title,
                    BalloonTipText = text
                    //BalloonTipText = text,
                    //BalloonTipIcon = icon
                };

                notify.ShowBalloonTip(3000);

                Task.Delay(3500).ContinueWith(_ =>
                {
                    try
                    {
                        notify.Visible = false;
                        notify.Dispose();
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationService] Error showing notification: {ex.Message}");
            }
        }
    }
}
