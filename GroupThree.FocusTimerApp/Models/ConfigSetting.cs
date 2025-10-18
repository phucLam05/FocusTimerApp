using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupThree.FocusTimerApp.Models
{
    public class ConfigSetting
    {
        public Dictionary<string, string> Hotkeys { get; set; } = new()
        {
            { "SwitchMode", "Alt+Q" },
            { "StartStop", "Alt+P" },
            { "OpenAlarm", "Alt+A" }
        };
    }
}
