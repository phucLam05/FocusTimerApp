using System;
using System.Media;
using System.Windows;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    /// <summary>
    /// Service responsible for displaying notifications to users
    /// Supports both visual notifications and sound alerts based on settings
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly SettingsService _settingsService;

        public NotificationService(SettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        /// <summary>
        /// Shows a notification with the specified title and message
        /// Respects user's notification settings (enable/disable, sound, etc.)
        /// </summary>
        public void ShowNotification(string title, string message)
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                var notificationSettings = settings.Notification ?? new NotificationSettings();

                // Check if notifications are enabled
                if (!notificationSettings.EnableNotifications)
                {
                    Console.WriteLine($"[Notification] Skipped (disabled): {title} - {message}");
                    return;
                }

                // Play sound if enabled
                if (notificationSettings.EnableSound)
                {
                    PlayNotificationSound();
                }

                // Show notification on UI thread
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    ShowWindowsNotification(title, message, notificationSettings);
                });

                Console.WriteLine($"[Notification] Shown: {title} - {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a specialized notification for timer completion events
        /// </summary>
        public void ShowTimerCompletionNotification(string timerType)
        {
            string title = "Timer Complete! ??";
            string message = timerType switch
            {
                "Work" => "Great job! Time for a break. ??",
                "ShortBreak" => "Break is over. Ready to focus again? ??",
                "LongBreak" => "Long break finished. Let's get back to work! ?",
                _ => $"{timerType} timer has completed."
            };

            ShowNotification(title, message);
        }

        /// <summary>
        /// Displays a Windows-style notification (can be MessageBox or Toast in future)
        /// </summary>
        private void ShowWindowsNotification(string title, string message, NotificationSettings settings)
        {
            try
            {
                // For now, using MessageBox. Can be replaced with Windows Toast notifications
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Auto-dismiss implementation would go here if using Toast notifications
                // For MessageBox, user must click OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] Display error: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a system beep sound for notification
        /// Can be extended to play custom sound files
        /// </summary>
        private void PlayNotificationSound()
        {
            try
            {
                // Play system beep sound
                SystemSounds.Beep.Play();
                
                // Alternative: Play a custom sound file
                // var player = new System.Media.SoundPlayer("path/to/sound.wav");
                // player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] Sound error: {ex.Message}");
            }
        }
    }
}
