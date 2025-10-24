using System;

namespace GroupThree.FocusTimerApp.Services
{
    /// <summary>
    /// Interface for notification service that handles displaying notifications to users
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Shows a notification with the specified title and message
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message content</param>
        void ShowNotification(string title, string message);

        /// <summary>
        /// Shows a notification for timer completion
        /// </summary>
        /// <param name="timerType">Type of timer that completed (e.g., "Work", "Break")</param>
        void ShowTimerCompletionNotification(string timerType);
    }
}
