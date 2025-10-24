using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupThree.FocusTimerApp.Models
{
    /// <summary>
    /// Root configuration model containing all application settings
    /// </summary>
    public class ConfigSetting
    {
        public TimerSettings TimerSettings { get; set; } = new();
        public ThemeSettings Theme { get; set; } = new();
        public List<HotkeyBinding> Hotkeys { get; set; } = new()
        {
            new HotkeyBinding
            {
                ActionName = "ToggleOverlay",
                Key = "Q",
                Modifiers = "Ctrl+Alt",
                Description = "Show/Hide overlay"
            },
            new HotkeyBinding
            {
                ActionName = "Start",
                Key = "W",
                Modifiers = "Ctrl+Alt",
                Description = "Start timer"
            },
            new HotkeyBinding
            {
                ActionName = "Pause",
                Key = "E",
                Modifiers = "Ctrl+Alt",
                Description = "Pause timer"
            },
            new HotkeyBinding
            {
                ActionName = "Stop",
                Key = "R",
                Modifiers = "Ctrl+Alt",
                Description = "Stop timer"
            }
        };
        public GeneralSettings General { get; set; } = new();
        public NotificationSettings Notification { get; set; } = new();
    }

    /// <summary>
    /// Timer configuration settings for Pomodoro and Tracking modes
    /// All durations are stored in minutes
    /// </summary>
    public class TimerSettings
    {
        /// <summary>
        /// Timer mode: "Pomodoro" for structured work/break cycles, "Tracking" for continuous time tracking
        /// </summary>
        public string Mode { get; set; } = "Pomodoro";
        
        /// <summary>
        /// Work/Focus session duration in minutes (default: 25 for Pomodoro technique)
        /// </summary>
        public int WorkDuration { get; set; } = 25;
        
        /// <summary>
        /// Short break duration in minutes (default: 5)
        /// </summary>
        public int BreakDuration { get; set; } = 5;
        
        /// <summary>
        /// Long break duration in minutes (default: 15)
        /// Taken after completing multiple work cycles
        /// </summary>
        public int LongBreakDuration { get; set; } = 15;
        
        /// <summary>
        /// Number of work cycles before taking a long break (default: 4)
        /// Example: Work-Break-Work-Break-Work-Break-Work-LongBreak
        /// </summary>
        public int LongBreakEvery { get; set; } = 4;
        
        /// <summary>
        /// Interval in minutes for tracking mode notifications (default: 15)
        /// Prompts user to record what they're working on
        /// </summary>
        public int TrackingInterval { get; set; } = 15;
        
        /// <summary>
        /// Whether to enable notifications for timer events (deprecated - use NotificationSettings instead)
        /// </summary>
        public bool EnableNotifications { get; set; } = true;
    }

    /// <summary>
    /// Theme configuration settings
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>
        /// Current application theme: "Light" or "Dark"
        /// </summary>
        public string AppTheme { get; set; } = "Light";
    }

    /// <summary>
    /// General application behavior settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Whether to start application automatically with Windows
        /// </summary>
        public bool StartWithWindows { get; set; } = false;
        
        /// <summary>
        /// Whether to minimize to system tray instead of closing
        /// </summary>
        public bool RunInBackground { get; set; } = true;
    }

    /// <summary>
    /// Notification behavior and preferences
    /// </summary>
    public class NotificationSettings
    {
        /// <summary>
        /// Master switch for all notifications
        /// </summary>
        public bool EnableNotifications { get; set; } = true;
        
        /// <summary>
        /// Whether to play sound with notifications
        /// </summary>
        public bool EnableSound { get; set; } = true;
        
        /// <summary>
        /// Whether notifications should automatically close after a timeout
        /// </summary>
        public bool AutoDismissNotifications { get; set; } = true;
        
        /// <summary>
        /// Whether notifications appear on all virtual desktops/workspaces
        /// </summary>
        public bool ShowOnAllWorkspaces { get; set; } = false;
    }
}