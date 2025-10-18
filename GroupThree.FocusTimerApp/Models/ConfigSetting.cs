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
        public List<HotkeyBinding> Hotkeys { get; set; } = new();
    }

    public class TimerSettings
    {
        public string Mode { get; set; } = "Pomodoro";
        public int WorkDuration { get; set; } = 50;
        public int BreakDuration { get; set; } = 10;
        public bool EnableNotifications { get; set; } = true;
    }

    public class ThemeSettings
    {
        public string AppTheme { get; set; } = "Light";
    }
}
