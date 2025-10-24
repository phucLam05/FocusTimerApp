using GroupThree.FocusTimerApp.Models;
using System.Collections.Generic;

namespace GroupThree.FocusTimerApp.Services
{
    public interface ISettingsService
    {
        ConfigSetting LoadSettings();
        void SaveSettings(ConfigSetting settings);
        List<HotkeyBinding> LoadHotkeys();
        void UpdateHotkeys(List<HotkeyBinding> newHotkeys);
        void ResetToDefault();
    }
}
