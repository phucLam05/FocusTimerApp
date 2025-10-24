using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupThree.FocusTimerApp.Models
{
    public class ConfigSetting
    {
        public TimerSettings TimerSettings { get; set; } = new();
        public ThemeSettings Theme { get; set; } = new();
        public List<HotkeyBinding> Hotkeys { get; set; } = new()
        {
            new HotkeyBinding
            {
                ActionName = "ToggleOverlay",
                Key = "P",
                Modifiers = "Ctrl+Alt",
                Description = "Show/Hide overlay"
            },
            new HotkeyBinding
            {
                ActionName = "Start",
                Key = "",
                Modifiers = "",
                Description = "Start timer"
            },
            new HotkeyBinding
            {
                ActionName = "Pause",
                Key = "",
                Modifiers = "",
                Description = "Pause timer"
            },
            new HotkeyBinding
            {
                ActionName = "Stop",
                Key = "",
                Modifiers = "",
                Description = "Stop timer"
            }
        };
        public GeneralSettings General { get; set; } = new();
        public NotificationSettings Notification { get; set; } = new();
    }

    public class TimerSettings
    {
        // Mode can be "Pomodoro" or "Basic"
        public string Mode { get; set; } = "Pomodoro";
        // Work duration in minutes (for Pomodoro work)
        public int WorkDuration { get; set; } = 50;
        // Break duration in minutes (short break)
        public int BreakDuration { get; set; } = 10;
        // Interval in minutes for tracking/basic notifications
        public int TrackingInterval { get; set; } = 15;
        // Whether to enable notifications for timer events
        public bool EnableNotifications { get; set; } = true;
    }

    public class ThemeSettings
    {
        public string AppTheme { get; set; } = "Light";
    }

    public class GeneralSettings
    {
        public bool StartWithWindows { get; set; } = false;
        public bool RunInBackground { get; set; } = true;
    }

    public class NotificationSettings
    {
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSound { get; set; } = true;
    }
}