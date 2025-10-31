public static class NotificationService
{
    public static void Show(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        var notify = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Information,
            Visible = true,
            BalloonTipTitle = title,
            BalloonTipText = text,
            BalloonTipIcon = icon
        };

        notify.ShowBalloonTip(3000);

        Task.Delay(3500).ContinueWith(_ =>
        {
            notify.Visible = false;
            notify.Dispose();
        });
    }
}
