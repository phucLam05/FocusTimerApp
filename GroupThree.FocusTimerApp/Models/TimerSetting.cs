using Newtonsoft.Json;
using System.IO;

namespace GroupThree.FocusTimerApp.Models
{
    public class TimerSetting
    {
        // Basic
        public int WorkDuration { get; set; } = 180; // phút
        public int ReminderInterval { get; set; } = 45;

        // Pomodoro
        public int ShortBreak { get; set; } = 5;
        public int ShortBreakAfter { get; set; } = 40;
        public int LongBreak { get; set; } = 20;
        public int LongBreakAfterShortBreakCount { get; set; } = 2;

        // Options
        public bool ExcludeBreakFromWorkTime { get; set; } = true;
    }

    public static class SettingManager
    {
        private static string filePath = "settings.json";

        public static TimerSetting Load()
        {
            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<TimerSetting>(File.ReadAllText(filePath))!;
            return new TimerSetting();
        }

        public static void Save(TimerSetting setting)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(setting, Newtonsoft.Json.Formatting.Indented));
        }
    }


}
